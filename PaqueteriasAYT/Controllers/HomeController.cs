using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaqueteriasAYT.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Drawing.Printing;
using Newtonsoft.Json;
using PaqueteriasAYT.Helpers;
using System.Net;
using System.IO;
using RestSharp;
using Spire.Pdf;
using System.Management;

namespace PaqueteriasAYT.Controllers
{
  [Authorize]
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration Configuration)
    {
      _configuration = Configuration;
      _logger = logger;
      Console.WriteLine("cambio");
    }

    public IActionResult Index()
    {
      bool automaticPrinting = _configuration["AppConfiguration:Automatic"] == "true" ? true : false;
      return View(automaticPrinting);
    }

    public async Task<IActionResult> GetParcels()
    {
      //Console.WriteLine(_configuration.GetValue<string>("AppConfiguration:PrinterId"));
      string site = _configuration["AppConfiguration:Zone"];
      bool automaticPrinting = _configuration["AppConfiguration:Automatic"] == "true" ? true : false;
      var parcelList = JsonConvert.DeserializeObject<List<Parcel>>(await ApiRequest.GetApiJson("GetParcels/" + site));
      foreach (Parcel parcel in parcelList)
      {
        if (automaticPrinting && parcel.Status < 3)
        {
          JsonResult response = await UpdateParcel(parcel.Id, "print") as JsonResult;
          parcel.Status = 3;
        }
      }
      return Json(new { data = parcelList });
    }

    public async Task<IActionResult> HistoricalParcels()
    {
      string site = _configuration["AppConfiguration:Zone"];
      //Console.WriteLine(_configuration.GetValue<string>("AppConfiguration:PrinterId"));
      var parcelList = JsonConvert.DeserializeObject<List<Parcel>>(await ApiRequest.GetApiJson("GetHistorical/" + site));
      return Json(new { data = parcelList == null ? new List<Parcel>() : parcelList });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateParcel(long id, string action, string reason = "", string guide = "", string ov = "", string parcel = "")
    {
      String queryString;
      switch (action)
      {
        case "work":
          try
          {
            queryString = "UPDATE AYT_Paqueterias SET Status = 3 WHERE Id = '" + id + "'";
            await ApiRequest.PostToApi(queryString);
            return Json(new { success = true, message = "Se actualizo el estatus" });
          }
          catch (Exception e)
          {
            return Json(new { success = false, message = e.ToString() });
          }
        case "print":
          try
          {
            queryString = "SELECT Id,LabelHTML,Ov,Cot FROM AYT_Paqueterias WHERE Id = '" + id + "'";
            Parcel labelParcel = JsonConvert.DeserializeObject<Parcel>(await ApiRequest.GetApiJson("GetLabel/" + queryString));
            string labelHtml = labelParcel.LabelHTML, Ov = labelParcel.Ov, Cot = labelParcel.Cot;
            labelHtml = labelHtml.Replace(Cot, Ov);
            //Creacion de label en server
            var client = new RestClient("http://inax.aytcloud.com/inax/public/impresion-public-orden");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddCookie("PHPSESSID", "vp8bet5uktp91lm0smb95t8021");
            request.AddParameter("application/x-www-form-urlencoded", "id=" + Ov + "&labelHtml=" + labelHtml + "", ParameterType.RequestBody);
            client.Execute(request);
            //Creacion de la ov en server 
            client = new RestClient("http://inax.aytcloud.com/inax/public/impresion-public-orden?id=" + Ov);
            request = new RestRequest(Method.GET);
            request.AddCookie("PHPSESSID", "vp8bet5uktp91lm0smb95t8021");
            client.Execute(request);
            string url = "http://inax.aytcloud.com/paqueteriasOv/" + Ov + ".pdf";
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(url), Ov + ".pdf");
            url = "http://inax.aytcloud.com/paqueteriasOv/" + Ov + "-label.pdf";
            webClient.DownloadFile(new Uri(url), Ov + "-label.pdf");
            PdfDocument ovPdf = new PdfDocument();
            PdfDocument labelPdf = new PdfDocument();
            ovPdf.LoadFromFile(Directory.GetCurrentDirectory() + "\\" + Ov + ".pdf");
            ovPdf.PrintSettings.PrinterName = _configuration["AppConfiguration:PrinterId"];
            ovPdf.Print();
            labelPdf.LoadFromFile(Directory.GetCurrentDirectory() + "\\" + Ov + "-label.pdf");
            labelPdf.PrintSettings.PrinterName = _configuration["AppConfiguration:PrinterId"];
            labelPdf.Print();
            queryString = "UPDATE AYT_Paqueterias SET Status = 3 WHERE Id = '" + id + "'";
            await ApiRequest.PostToApi(queryString);
            return Json(new { success = true, message = "Impresiones enviadas correctamente" });
          }
          catch (Exception e)
          {
            return Json(new { success = false, message = e.ToString() });
          }
        case "guide":
          try
          {

            string query = "SalesOrderHeadersV2?%24" +
                "select=OrderTakerPersonnelNumber,InvoiceCustomerAccountNumber&%24" +
                "filter=SalesOrderNumber%20eq%20'" + ov + "'";
            Dictionary<string, dynamic> ovTaker = await OdataConection.Query(query);
            query = "Employees?%24top=10&%24select=Education&%24filter=PersonnelNumber%20eq%20'" + ovTaker["value"][0].OrderTakerPersonnelNumber + "'";
            Dictionary<string, dynamic> takerEducation = await OdataConection.Query(query);
            query = "SystemUsers?%24select=Email&%24filter=UserID%20eq%20'" + takerEducation["value"][0].Education + "'";
            Dictionary<string, dynamic> takerMail = await OdataConection.Query(query);
            query = "CustomersV3?%24" +
                "select=PrimaryContactEmail&%24" +
                "filter=CustomerAccount%20eq%20'" + ovTaker["value"][0].InvoiceCustomerAccountNumber + "'";
            Dictionary<string, dynamic> clientMail = await OdataConection.Query(query);
            //["value"][0].ReleaseStatus;
            queryString = "UPDATE AYT_Paqueterias SET Status = 5, GuideNumber = '" + guide + "', ParcelCompany = '" + parcel + "'   WHERE Id = '" + id + "'";
            string clientEmail = clientMail["value"][0].PrimaryContactEmail != "" ? clientMail["value"][0].PrimaryContactEmail : "El cliente no cuenta con correo";
            clientEmail = clientEmail.Replace(";", ",");
            string stringTakerMail = takerMail["value"].Count > 0 ? takerMail["value"][0].Email : "";
            string from = "notificaciones@avanceytec.com.mx", to = "", body, subject = "Numero de guia: " + ov;
            if (clientEmail != "El cliente no cuenta con correo")
            {
              to = clientEmail;
            }
            if (string.IsNullOrEmpty(to))
            {
              if (string.IsNullOrEmpty(stringTakerMail))
              {
                to = "sistemas13@avanceytec.com.mx";
              }
              else
              {
                to = stringTakerMail;
              }
            }
            else
            {
              if (!string.IsNullOrEmpty(stringTakerMail))
              {
                to += "," + stringTakerMail;
              }
            }
            body = "<h2>Avance y tecnologia en plasticos</h2>";
            //Console.WriteLine(clientMail["value"][0].PrimaryContactEmail);
            body += "<h4>Correo Cliente: " + clientEmail + "</h4>";
            body += "<h4>Se genero un numero de guia: " + guide + "</h4>";
            body += "<h4>Orden de venta: " + ov + "</h4>";
            body += "<h4>Paqueteria: " + parcel + "</h4>";
            body += "<table style=\"width:100%;border:1px solid black; padding:5px;\" cellspadding=\"3\" border=\"1\"><tr>" +
                "<th style=\"background-color: #334f8b; color: White; text-align:left; padding:5px;\">Descripción</th>" +
                "<th style=\"background-color: #334f8b; color: White; text-align:center; padding:5px;\">Cantidad</th></tr>";
            query = "AYT_SalesLines?%24" +
                "select=ItemId,Name,SalesQty,SalesUnit&%24" +
                "filter=SalesId%20eq%20'" + ov + "'";
            var serializedObject = await OdataConection.QueryJson(query);
            var deserializedObject = JsonConvert.DeserializeObject<SalesLinesJsonObject>(serializedObject);
            foreach (SalesOrderLines line in deserializedObject.value)
            {
              body += "<tr><td style=\"width: 90 %; padding: 5px;\">" + line.Name + "</td>" +
              "<td style = \"width:10%; text-align:center;\" > " + line.SalesQty + " </td></tr>";
            }
            body += "</table><h3>Favor de no responder sobre este correo, es unicamente para notificaciones.</h3>";
            SendMail.Send(from, to, body, subject);
            await ApiRequest.PostToApi(queryString);
            //PRESTASHOP
            //try para evitar que falle paqueterias por prestashop
            try
            {
              var queryPrestashop = $"?OrdenVenta={ov}&guias={guide}";
              var api = "Prestashop";
              await ApiRequest.PutToPrestashop(api, queryPrestashop);
            }
            catch (Exception)
            {
              throw;
            }
            return Json(new { success = true, message = "Se actualizo la guia" });
          }
          catch (Exception e)
          {
            return Json(new { success = false, message = e.ToString() });
          }
        case "report":
          try
          {
            queryString = "UPDATE AYT_Paqueterias SET Status = 4, StopReason = '" + reason + "'   WHERE Id = '" + id + "'";
            await ApiRequest.PostToApi(queryString);
            return Json(new { success = true, message = "Se detuvo el proceso " });
          }
          catch (Exception e)
          {
            return Json(new { success = false, message = e.ToString() });
          }
        default:
          return Json(new { success = false, message = "Update Action Not Found" });

      }

    }

    [HttpPost]
    public async Task<IActionResult> RePrint(int id, int ovCopies, int labelCopies, int zebraCopies)
    {
      String queryString;
      try
      {
        queryString = "SELECT Id,LabelHTML,Ov,Cot FROM AYT_Paqueterias WHERE Id = '" + id + "'";
        Parcel labelParcel = JsonConvert.DeserializeObject<Parcel>(await ApiRequest.GetApiJson("GetLabel/" + queryString));
        string labelHtml = labelParcel.LabelHTML, Ov = labelParcel.Ov, Cot = labelParcel.Cot;
        labelHtml = labelHtml.Replace(Cot, Ov);
        labelHtml = labelHtml.Replace("&amp;", "");
        //Creacion de label en server
        var client = new RestClient("http://inax.aytcloud.com/inax/public/impresion-public-orden");
        var request = new RestRequest(Method.POST);
        request.AddHeader("content-type", "application/x-www-form-urlencoded");
        request.AddCookie("PHPSESSID", "vp8bet5uktp91lm0smb95t8021");
        request.AddParameter("application/x-www-form-urlencoded", "id=" + Ov + "&labelHtml=" + labelHtml + "", ParameterType.RequestBody);
        client.Execute(request);
        //Creacion de la ov en server
        client = new RestClient("http://inax.aytcloud.com/inax/public/impresion-public-orden?id=" + Ov);
        request = new RestRequest(Method.GET);
        request.AddCookie("PHPSESSID", "vp8bet5uktp91lm0smb95t8021");
        client.Execute(request);
        string url = "http://inax.aytcloud.com/paqueteriasOv/" + Ov + ".pdf";
        WebClient webClient = new WebClient();
        webClient.DownloadFile(new Uri(url), Ov + ".pdf");
        url = "http://inax.aytcloud.com/paqueteriasOv/" + Ov + "-label.pdf";
        webClient.DownloadFile(new Uri(url), Ov + "-label.pdf");
        url = "http://inax.aytcloud.com/paqueteriasOv/"+Cot+".zpl";
        Boolean existeZPL = true;
        try
        {
          webClient.DownloadFile(new Uri(url), Cot + ".zpl");
          ReplaceCotByOv(Cot + ".zpl", Cot, Ov,zebraCopies);
        }
        catch (WebException e)
        {
          var statusCode = ((HttpWebResponse)e.Response).StatusCode;

          if (statusCode == HttpStatusCode.NotFound && System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\" + Cot + ".zpl"))
          {
            existeZPL = false;
          }
        }
        PdfDocument ovPdf = new PdfDocument();
        PdfDocument labelPdf = new PdfDocument();
        ovPdf.LoadFromFile(Directory.GetCurrentDirectory() + "\\" + Ov + ".pdf");
        ovPdf.PrintSettings.PrinterName = _configuration["AppConfiguration:PrinterId"];
        if (ovCopies > 0)
        {
          ovPdf.PrintSettings.Copies = (short) ovCopies;
          ovPdf.Print();
        }
        labelPdf.LoadFromFile(Directory.GetCurrentDirectory() + "\\" + Ov + "-label.pdf");
        labelPdf.PrintSettings.PrinterName = _configuration["AppConfiguration:PrinterId"];
        String printerName = _configuration["AppConfiguration:PrinterZPLId"];
        String[] tipo = GetPrinterDriverName(printerName);
        if (labelCopies > 0)
        {
          labelPdf.PrintSettings.Copies = (short) labelCopies;
          labelPdf.Print();
        }
        try
        {
          if (tipo[0] == "zebra" && existeZPL && !string.IsNullOrEmpty(_configuration["AppConfiguration:PrinterZPLId"]) && zebraCopies > 0)
          {
            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            var st = "/c copy /B " + Directory.GetCurrentDirectory() + "\\" + Cot + ".zpl \"" + tipo[1] + "\"";
            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", st);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();
            // Display the command output.
            Console.WriteLine(result);
          }
        }
        catch (Exception objException)
        {
          // Log the exception
          Console.WriteLine(objException.Message);
        }
        queryString = "UPDATE AYT_Paqueterias SET Status = 3 WHERE Id = '" + id + "'";
        await ApiRequest.PostToApi(queryString);
        return Json(new { success = true, message = "Impresiones enviadas correctamente" });
      }
      catch (Exception e)
      {
        return Json(new { success = false, message = e.ToString() });
      }
    }

    public String[] GetPrinterDriverName(String printerName)
    {
      ////////////////////
      string query2 = string.Format("SELECT * FROM Win32_Printer");
      ManagementObjectSearcher searcher = new ManagementObjectSearcher(query2);
      ManagementObjectCollection coll = searcher.Get();

      String Driver = "unknown";
      String DestinationPrinterID = String.Empty;
      foreach (ManagementObject printer in coll)
      {
        if (printer.Properties["Name"].Value.ToString() == printerName)
        {
          var property = printer.Properties["DriverName"];
          if (property.Value.ToString().ToLowerInvariant().Contains("zebra") || property.Value.ToString().ToLowerInvariant().Contains("zdesigner"))
          {
            Driver = "zebra";
            DestinationPrinterID = printer.Properties["Name"].Value.ToString();
            if (printer.Properties["Local"].Value.ToString() == "True")
            {
              DestinationPrinterID = "\\\\" + printer.Properties["SystemName"].Value.ToString() + "\\" + printer.Properties["ShareName"].Value.ToString();
            }
          }
          else
          {
            Driver = "regular";
          }
        }
      }
      String[] result = new []{ Driver, DestinationPrinterID };
      return result;
      //////////////////////////////////////////////
    }

    public void ReplaceCotByOv(String fileName, String buscar, String remplazo, int zebraCopies)
    {
      StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\" + fileName);
      string input = reader.ReadToEnd();
      reader.Close();

      using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\" + fileName, false))
      {
        {
          string output = input.Replace(buscar, remplazo);
          remplazo = remplazo.Substring(remplazo.Length-6);
          output = output.Replace("{Ov}", remplazo);
          output = output.Replace("{Copies}", zebraCopies.ToString());
          writer.Write(output);
        }
        writer.Close();
      }
    }

    public IActionResult ChangeAutomatic()
    {
      bool automaticPrinting = _configuration["AppConfiguration:Automatic"] == "true" ? true : false;
      if (automaticPrinting)
        _configuration["AppConfiguration:Automatic"] = "false";
      else
        _configuration["AppConfiguration:Automatic"] = "true";

      return Json(new { success = true, message = "Impresiones Automaticas Iniciadas!", automatic = _configuration["AppConfiguration:Automatic"] == "true" ? true : false });
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
  public class SalesLinesJsonObject
  {
    public Dictionary<string, string> @odatacontext { get; set; }
    public List<SalesOrderLines> value { get; set; }
  }
}
