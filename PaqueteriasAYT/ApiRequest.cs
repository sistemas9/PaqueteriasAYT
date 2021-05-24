using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT
{
  public class ApiRequest
  {
    private static IConfiguration _configuration = Startup.StaticConfig;
    public static async Task<string> GetApiJson(string query)
    {
      var client = new RestClient(_configuration["ApiConectionEnviroment"] + query);
      var request = new RestRequest(Method.GET);
      request.AddCookie(".AspNetCore.Antiforgery.XT6nEUiSeek", "CfDJ8AhqgK3czbJLjqzYhQiMaH_TrBZZcCf-eQ74T813xl-VDkXZYIFNZLxEQ_M9_bLJ8do1-ogXgQqPwswclht26EPwi3FyxUzjKgQphPezPU9_oztW9btiRoEO21kv7tALRpDivWdpXNWpeU1UvwmV11Q");
      IRestResponse response = await client.ExecuteAsync(request);
      return response.Content;
    }
    public static async Task PostToApi(string query, string postAction = "PostQuery/")
    {
      var client = new RestClient(_configuration["ApiConectionEnviroment"] + postAction + query);
      var request = new RestRequest(Method.GET);
      request.AddCookie(".AspNetCore.Antiforgery.XT6nEUiSeek", "CfDJ8AhqgK3czbJLjqzYhQiMaH_TrBZZcCf-eQ74T813xl-VDkXZYIFNZLxEQ_M9_bLJ8do1-ogXgQqPwswclht26EPwi3FyxUzjKgQphPezPU9_oztW9btiRoEO21kv7tALRpDivWdpXNWpeU1UvwmV11Q");
      IRestResponse response = await client.ExecuteAsync(request);
    }
    public static async Task PutToPrestashop(string api, string query)
    {
      var client = new RestClient($"{ _configuration["ApiConnection"]}{api}{query}");
      var request = new RestRequest(Method.PUT);
      IRestResponse response = await client.ExecuteAsync(request);
    }

  }
}
