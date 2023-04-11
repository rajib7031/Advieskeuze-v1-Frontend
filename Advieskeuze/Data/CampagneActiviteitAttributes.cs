using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System;

namespace Advieskeuze.Data {
  /// <summary>
  /// Attribuut om op action level te waarschuwen voor een cache. 
  /// (Waardoor er geen campagneID's in linkjes belanden die dan in de cache zouden komen)
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class CampagneActiviteitCacheWarningAttribute : System.Web.Mvc.ActionFilterAttribute {
    public CampagneActiviteitCacheWarningAttribute() {
      Order = 2;  // Uitvoeren na het activeren van het environment
    }

    public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext) {
      // Oud Sla dit op in de CampagneActiviteit
      CampagneActiviteitContext.Current.ActivateCacheWarning();
      // Gewenste situatie
      var controller = filterContext.Controller as BaseController;
      if (controller != null)
        controller.SetCacheWarning();
      base.OnActionExecuting(filterContext);
    }
  }


  /// <summary>
  /// Bepaalt de scope zodat de campagne (beperkte) delen van de site kan targeten
  /// </summary>
  /// <remarks>usescope via querystring kan de hier opgeven scope weer overschrijven</remarks>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class SetCampagneActiviteitScopeAttribute : System.Web.Mvc.ActionFilterAttribute {
    private readonly CampagneActiviteitScope _useThisScope;
    public SetCampagneActiviteitScopeAttribute(CampagneActiviteitScope useThisScope) {
      Order = 2;  // Uitvoeren na het activeren van het environment
      _useThisScope = useThisScope;
    }

    public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext) {
      // Oud Sla dit op in de CampagneActiviteit
      CampagneActiviteitContext.Current.SetScope(_useThisScope);
      // Gewenste situatie
      var controller = filterContext.Controller as BaseController;
      if (controller != null)
        controller.SetScope(_useThisScope);
      base.OnActionExecuting(filterContext);
    }
  }

}