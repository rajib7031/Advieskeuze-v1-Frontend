using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using SharedCode;
using SharedCode.Mvc;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Advieskeuze {
  public class MvcApplication : HttpApplication {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
    }

    public static void RegisterRoutes(RouteCollection routes) {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.MapRouteLowercase(
        "SEO",
        "over/nederland/{productgroep}/{provincie}/{gemeente}/{plaats}",
        new { controller = "over", action = "nederland", provincie = UrlParameter.Optional, gemeente = UrlParameter.Optional, plaats = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "Aanbieder",
        "over/aanbieder/{id}",
        new { controller = "Over", action = "Aanbieder" },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "Aanbieders",
        "over/{productgroep}/aanbieders",
        new { controller = "Over", action = "Aanbieders" },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "GoB",
        "go/b/{beoordelingmodule}/{id}/{organisatie}",
        new { controller = "Go", action = "B", organisatie = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "GoK",
        "go/k/{id}/{organisatie}",
        new { controller = "Go", action = "K", organisatie = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "GoA",
        "go/a/{id}/{productgroep}",
        new { controller = "Go", action = "A", productgroep = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "GoR",
        "go/r/{id}/{productgroep}",
        new { controller = "Go", action = "R", productgroep = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "GoBanner",
        "go/banner/{id}/{slug}/{properties}",
        new { controller = "Go", action = "Banner", properties = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      // Start: verouderde verwijzingen opvangen.
      routes.MapRouteLowercase(
        "GoWidget",
        "go/widget/{id}/{type}/{properties}",
        new { controller = "Go", action = "Widget", properties = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      // Einde: verouderde verwijzingen opvangen.
      routes.MapRouteLowercase(
        "GoC",
        "go/c/{campagneactiviteit}", // http://dev.advieskeuze.nl/go/c/AD4A30
        new { controller = "Go", action = "C", area = "home" },
        null,
        new[] { "Advieskeuze.Controllers" }
        );

      //routes.MapRouteLowercase(
      //  "zoekresultaat",
      //  "zoekmodule/{slug}/{action}",
      //  new { controller = "zoekmodule", action = "index" },
      //  null,
      //  new[] { "Advieskeuze.Controllers" }
      //  );

      routes.MapRouteLowercase(
        "zoekmodule",
        "zoekmodule/{slug}/{action}",
        new { controller = "zoekmodule", action = "index" },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
      routes.MapRouteLowercase(
        "Default",
        "{controller}/{action}/{id}",
        new { controller = "Home", action = "Index", id = UrlParameter.Optional },
        null,
        new[] { "Advieskeuze.Controllers" }
        );
    }

    protected virtual void Application_BeginRequest() {
      // The official <rewrite rules> trigger too soon for Azure to handle the request for multiple domains (www.ak.nl & ak.nl).
      // So to ensure SSL and WWW for every possible link/url, this rudimentary solution works.
      if (!GewrapteSettings.IsDev) {
        if (!Request.Url.Host.StartsWith("www") && !Request.Url.IsLoopback) {
          UriBuilder builder = new UriBuilder(Request.Url);
          builder.Host = "www." + Request.Url.Host;
          Response.StatusCode = 301;
          Response.AddHeader("Location", builder.ToString());
          Response.End();
        }
        if (!Request.IsSecureConnection) {
          Response.RedirectPermanent("https://" + Request.ServerVariables["HTTP_HOST"] + HttpContext.Current.Request.RawUrl);
        }
      }

      DataContext.RejectChangesAndReloadData(); // Refresh datacontext per request
      HttpContext.Current.Response.AddHeader("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
    }

    protected virtual void Application_EndRequest() {
      // EROS: Geen remove hier van datacontext, overbodig
    }

    protected void Application_Start() {
      AreaRegistration.RegisterAllAreas();
      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);
      DefaultModelBinder.ResourceClassKey = "ValidationMessages";
      AjaxHelper.GlobalizationScriptPath = "http://ajax.microsoft.com/ajax/4.0/1/globalization/";

      // Custom strings
      ClientDataTypeModelValidatorProvider.ResourceClassKey = "Messages";
      DefaultModelBinder.ResourceClassKey = "Messages";

      Localization.SetCultureNL();
      MvcHandler.DisableMvcResponseHeader = true;

      // Forceer de Razor engine (zodat er geen .ascx/.aspx meer worden gezocht)
      ViewEngines.Engines.Clear();
      ViewEngines.Engines.Add(new RazorViewEngine());

      var root = new CompositionRoot();
      ControllerBuilder.Current.SetControllerFactory(root.ControllerFactory);
    }

    /// <summary>
    /// Start de session. We maken geen actief gebruik meer van de session, daarom maakt hij ZONDER deze methode OF als er nog niets in de session staat elke keer een unieke sessionid aan.
    /// In het extranet is dit niet nodig omdat daar de session actief gebruikt wordt.
    /// http://www.ytechie.com/2008/07/aspnet-changing-session-ids-for-each-request/
    /// </summary>
    public void Session_Start(object sender, EventArgs e) { }
  }
}