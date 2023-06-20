using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaqueteriasAYT.Data;
using PaqueteriasAYT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT
{
  public class ConfigurationMiddleware
  {
    //private readonly ApplicationDbContext _db;
    public ConfigurationMiddleware(RequestDelegate next)
    {
      _next = next;
      //_db = db;
    }

    readonly RequestDelegate _next;

    public async Task Invoke(HttpContext context, IConfiguration configuration)
    {
      if (! await IsConfigured(configuration, context) && context.User.Identity.IsAuthenticated)
      {
        var url = "/Configuration";
        //check if the current url contains the path of the installation url
        if (context.Request.Path.Value.IndexOf(url, StringComparison.CurrentCultureIgnoreCase) == -1 && context.Request.Path.Value.IndexOf("/AzureAD/Account/SignOut", StringComparison.CurrentCultureIgnoreCase) == -1)
        {
          context.Response.Redirect(url);
          return;
        }
      }
      //or call the next middleware in the request pipeline
      await _next(context);
    }

    public async Task<bool> IsConfigured(IConfiguration configuration, HttpContext context)
    {
      var configuredPrinter = configuration.GetValue<string>("AppConfiguration:PrinterId");
      var configuredPrinterZPL = configuration.GetValue<string>("AppConfiguration:PrinterZPLId");
      var configuredZone = configuration.GetValue<string>("AppConfiguration:Zone");
      if (string.IsNullOrEmpty(configuredPrinter) || string.IsNullOrEmpty(configuredZone) || string.IsNullOrEmpty(configuredPrinterZPL))
      {                
        string queryString = "SELECT * FROM AppConfiguration WHERE [User] = '" + context.User.Identity.Name + "'";
        AppConfiguration appConfiguration = JsonConvert.DeserializeObject<AppConfiguration>(await ApiRequest.GetApiJson("GetConfiguration/" + queryString));
        Console.WriteLine("GetConfiguration/" + queryString);
        if (appConfiguration.User == null || appConfiguration.Id == 0 || appConfiguration.PrinterPath == null)
            return false;
        configuration["AppConfiguration:Zone"] = appConfiguration.SiteId;
        configuration["AppConfiguration:PrinterId"] = @"" + appConfiguration.PrinterPath;
        configuration["AppConfiguration:PrinterZPLId"] = @"" + appConfiguration.PrinterZPLPath;
      }
      return (!string.IsNullOrEmpty(configuration.GetValue<string>("AppConfiguration:PrinterId")) && !string.IsNullOrEmpty(configuration.GetValue<string>("AppConfiguration:Zone"))) ? true : false;
      //Console.WriteLine(_db);
      //var savedConfiguration = _db.AppConfiguration.Count(x => (x.User == context.User.Identity.Name));
      //Console.WriteLine(savedConfiguration);
    }
  }
}
