using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Routing;

namespace Advieskeuze.Data {
  /// <summary>
  /// Laad de advieskeuze omgeving in op de controller. 
  /// Bevat de productgroep, ps code (zoekresultaat), c code (campagneactiviteit) en sitemap.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class AdvieskeuzeEnvironmentAttribute : ActionFilterAttribute {
    public AdvieskeuzeEnvironmentAttribute() {
      Order = 1;  // Altijd als eerste uitvoeren
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext) {
      var controller = filterContext.Controller as BaseController;
      if (controller != null) {
        var routeData = filterContext.RouteData;
        var request = filterContext.HttpContext.Request;
        var queryString = request.QueryString;
        var formFiels = request.Form;
        LaadEnvironment(controller, routeData, queryString, formFiels);
      }
      base.OnActionExecuting(filterContext);
    }

    private static void LaadEnvironment(BaseController opController, RouteData routeData, NameValueCollection queryString, NameValueCollection formFields) {
      var routeProductgroeparea = string.Empty;
      if (routeData.Values.ContainsKey("productgroeparea")) {
        routeProductgroeparea = routeData.Values["productgroeparea"] + "";
        if (routeProductgroeparea.ToLower() == "productgroepalgemeen")
          routeProductgroeparea = string.Empty;
      }
      var routeAreaAdvies = string.Empty;
      if (routeData.Values.ContainsKey("areaadvies")) {
        routeAreaAdvies = routeData.Values["areaadvies"] + "";
        if (routeAreaAdvies.ToLower() == AdvieskeuzeCode.DataModel.Scope.SlugAdviesDefault)
          routeAreaAdvies = string.Empty;
      }
      var routeArea = string.Empty;
      if (routeData.Values.ContainsKey("area")) {
        routeArea = routeData.Values["area"] + "";
      }
      var routeProductgroep = string.Empty;
      if (routeData.Values.ContainsKey("productgroep")) {
        routeProductgroep = routeData.Values["productgroep"] + "";
      }
      var routeBeoordelingmodule = string.Empty;
      if (routeData.Values.ContainsKey("beoordelingmodule")) {
        routeBeoordelingmodule = routeData.Values["beoordelingmodule"] + "";
      }
      var routeThema = string.Empty;
      if (routeData.Values.ContainsKey("themaarea")) {
        routeThema = routeData.Values["themaarea"] + "";
        if (routeThema.ToLower() == "themaarea")
          routeThema = string.Empty;
      }
      var campagneActiviteitFormField = formFields["postcampagneactiviteit"];  // Form voor post compatibility
      var campagneActiviteitQueryString = queryString[AdvieskeuzeCode.Routes.CampagneActiviteitContext.CampagneActiviteitQueryStringName];  // GET voor link compatibility
      opController.LaadEnvironment(routeArea, routeAreaAdvies, routeProductgroeparea, routeThema, routeProductgroep, routeBeoordelingmodule, campagneActiviteitQueryString, campagneActiviteitFormField);
    }
  }
}
