using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaqueteriasAYT.Data;

namespace PaqueteriasAYT
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        StaticConfig = configuration;
    }

    public static IConfiguration StaticConfig { get; private set; }
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      //services.Configure<CookiePolicyOptions>(options =>
      //{
      //  // This lambda determines whether user consent for 
      //  //non -essential cookies is needed for a given request.
      //  //options.CheckConsentNeeded = context => true;
      //  options.MinimumSameSitePolicy = SameSiteMode.Strict;
      //});
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
      .AddAzureAD(options =>
        {
          Configuration.Bind("AzureAd", options);
        }
      );

      services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")));
      //services.AddAuthorization(options => {
      //    options.AddPolicy("Administrator",
      //            policyBuilder => policyBuilder.RequireClaim("groups",
      //            "78bcf2fd-3fde-4587-a71c-095b6ffb38a9"));
      //});

      services.AddControllersWithViews(options =>
      {
          var policy = new AuthorizationPolicyBuilder()
              .RequireAuthenticatedUser()
              .Build();
          options.Filters.Add(new AuthorizeFilter(policy));
      });
      services.AddRazorPages().AddRazorRuntimeCompilation();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
    {
      //CookiePolicyOptions optCookie = new CookiePolicyOptions();
      //optCookie.MinimumSameSitePolicy = SameSiteMode.None;
      //app.UseCookiePolicy(optCookie);
      if (env.IsDevelopment())
      {
          app.UseDeveloperExceptionPage();
      }
      else
      {
          app.UseExceptionHandler("/Home/Error");
          // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
          app.UseHsts();
      }
      
      //app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseRouting();
      app.UseCookiePolicy(new CookiePolicyOptions
      {
        Secure = CookieSecurePolicy.Always
      });
      app.UseAuthentication();
      app.UseAuthorization();
      app.UseMiddleware<ConfigurationMiddleware>();
      app.UseEndpoints(endpoints =>
      {
          endpoints.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");
          endpoints.MapRazorPages();
      });
      appLifetime.ApplicationStarted.Register(() => OpenBrowser());
    }
    private static void OpenBrowser()
    {
      Process.Start(
      new ProcessStartInfo("cmd", $"/c start Firefox --new-window \"http://localhost:5000/\"")
      {
        CreateNoWindow = true
      });
      //Process.Start(
      //new ProcessStartInfo("cmd", $"/c start Chrome --new-window \"http://localhost:5000/\"")
      //{
      //  CreateNoWindow = true
      //});
    }
  }
}
