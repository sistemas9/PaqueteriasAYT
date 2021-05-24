using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using System.Drawing.Printing;
using System.IO;
using PaqueteriasAYT.Models;
using PaqueteriasAYT.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Management;
using System.Security.Policy;

namespace PaqueteriasAYT.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IConfiguration _configuration;
        public ConfigurationController(ApplicationDbContext db, IConfiguration Configuration)
        {
            _configuration = Configuration;
            _db = db;
        }
        public async  Task<IActionResult> Index()
        {
            AppConfiguration emptyConfiguration = new AppConfiguration { SiteId = "", PrinterPath = "" };
            ViewData["configuration"] = emptyConfiguration;
            string queryString = "SELECT * FROM AppConfiguration WHERE [User] = '" + User.Identity.Name + "'";
            AppConfiguration appConfiguration = JsonConvert.DeserializeObject<AppConfiguration>(await ApiRequest.GetApiJson("GetConfiguration/" + queryString));
            if (appConfiguration.User != null || appConfiguration.Id != 0 || appConfiguration.PrinterPath != null)
                ViewData["configuration"] = appConfiguration;

            List<string> printerList = new List<string>();

            foreach (var printer in PrinterSettings.InstalledPrinters)
            {
              printerList.Add(printer.ToString());
            }



      ///////////////////network printers/////////////
      ////Use the ObjectQuery to get the list of configured printers
      //var oquery = new System.Management.ObjectQuery("SELECT * FROM Win32_Printer");

      //var mosearcher = new System.Management.ManagementObjectSearcher(oquery);

      //System.Management.ManagementObjectCollection moc = mosearcher.Get();

      //foreach (ManagementObject mo in moc)
      //{
      //  System.Management.PropertyDataCollection pdc = mo.Properties;

      //  foreach (System.Management.PropertyData pd in pdc)
      //  {
      //    //if ((bool)mo["Network"])
      //    //{
      //    if (pd.Name.Equals("DeviceID") || pd.Name.Equals("DriverName") || pd.Name.Equals("Local") 
      //      || pd.Name.Equals("Name") || pd.Name.Equals("Network") || pd.Name.Equals("ServerName") || pd.Name.Equals("Shared")
      //      || pd.Name.Equals("ShareName") || pd.Name.Equals("SystemName"))
      //    {
      //      Console.WriteLine(pd.Name + " " + pd.Value);
      //    }
      //    //if (mo[pd.Name].ToString() == "Attributes")
      //    //{
      //    //  //printerList.Add(mo[pd.Name].ToString());
      //    //  Console.WriteLine(mo[pd.Name].ToString());
      //    //}
      //    //}
      //  }
      //  Console.WriteLine("----------------------\r\n");
      //}
      ///////////////////////////////////////////////

      string query = "OperationalSites";
            Dictionary<string, dynamic> sites = await OdataConection.Query(query);
            List<StoreSite> sitesArray = new List<StoreSite>();
            foreach(var site in sites["value"])
            {
                sitesArray.Add(new StoreSite{ siteId = site.SiteId, siteName = site.SiteName });
            }
            ViewData["sites"] = sitesArray;
            return View(printerList);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string site, string printer, string printerZPL)
        {
            _configuration["AppConfiguration:PrinterId"] = @"" + printer;
            _configuration["AppConfiguration:PrinterZPLId"] = @"" + printerZPL;
            _configuration["AppConfiguration:Zone"] = site;
            string queryString = "SELECT * FROM AppConfiguration WHERE [User] = '" + User.Identity.Name + "'";
            AppConfiguration appConfiguration = JsonConvert.DeserializeObject<AppConfiguration>(await ApiRequest.GetApiJson("GetConfiguration/" + queryString));
            if (appConfiguration.User != null || appConfiguration.Id != 0 || appConfiguration.PrinterPath != null || appConfiguration.PrinterZPLPath != null)
            {
                queryString = "UPDATE AppConfiguration SET [SiteId] = '"+site+ "', [PrinterPath] = '"+printer+"', [PrinterZPLPath] = '"+printerZPL+"' WHERE Id ="+appConfiguration.Id;
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(queryString);
                await ApiRequest.PostToApi(Convert.ToBase64String(plainTextBytes), "PostConfigurationQuery/");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                queryString = "INSERT INTO AppConfiguration ([SiteId],[PrinterPath],[User],[PrinterZPLPath]) " +
                    "VALUES('"+site+"','"+printer+"','" +User.Identity.Name+ "', '"+printerZPL+"');";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(queryString);
                await ApiRequest.PostToApi(Convert.ToBase64String(plainTextBytes), "PostConfigurationQuery/");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}