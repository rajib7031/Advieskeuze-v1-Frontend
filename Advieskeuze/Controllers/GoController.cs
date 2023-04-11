using Advieskeuze.Data;
using AdvieskeuzeCode.Banners;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode;
using SharedCode.Mvc;
using System;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  /// <summary>
  /// Verkorte URL's voor continu support.
  /// </summary>
  /// De 'go' controller is bedoeld als springplank naar functionaliteit elders in de website. 
  /// 
  /// Door het afschermen van de target is het eenvoudiger om de routings functionerend te houden.
  /// 
  /// Het campagneactiviteit mechanisme speelt hier een belangrijke rol. Dat is omdat via dit
  /// mechanisme bekend is wat de target is maar ook wat belangrijke omstandigheden zijn. Zo
  /// kan je via de campagne de organisatie achterhalen, de productgroep, de zoekmodule en
  /// het expert platform.
  /// 
  /// Een deel is ook gewijd aan de banners (vroeger 'widgets' genoemd). De banner komt via het
  /// CDN naar de website via deze go controller. Kantoren hebben (als het goed is) alleen een
  /// verwijzing naar of het cdn of naar deze go controller.
  /// <remarks>
  /// Het is de truc om hier snel te zijn. Dat betekend niet te veel in de database zoeken zodat
  /// de latency laag is. Dit is maar een springplank; tijd kan beter in de content zitten.
  /// </remarks>
  [AdvieskeuzeEnvironment]
  public class GoController : BaseController {

    /// <summary>
    /// Go to Beoordeling with InternID or ExternID.
    /// http://dev.advieskeuze.nl/go/b/hypotheek/2d6472fe-0043-435a-b184-b968c28584f5/organisatienaam
    /// </summary>
    /// <param name="ke">Klant e-mailadres</param>
    /// <param name="kn">Klant naam</param>
    [SetCampagneActiviteitScope(CampagneActiviteitScope.Beoordeling)]
    public RedirectToRouteResult B(string beoordelingmodule, string id, string organisatie = null, string ke = null, string an = null, string kn = null, string c = null, bool k = false, int? w = null) {
      DomainContext.GetEnvironment().ApplyCampagneActiviteit(Environment, c);
      Guid locatieID;
      if (Guid.TryParse(id, out locatieID)) {
        var locatie = DomainContext.GetLocaties().GetVoorLinkByInternOrExternId(locatieID);
        if (locatie != null) {
          var beoordelingmoduleData = Environment.Beoordelingmodule;
          var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
          BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope, locatie.ID);
          return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(beoordelingmoduleData, locatie, an, false, ke, kn, w));
        }
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling());
    }

    /// <summary>
    /// Go to CampagneActiviteit with ExternID.
    /// http://dev.advieskeuze.nl/go/c/abcdef
    /// </summary>
    public RedirectToRouteResult C(string campagneactiviteit) {
      var ca = Environment.CampagneActiviteit;
      if (Environment.HeeftCampagneActiviteit && !Environment.CampagneActiviteit.Data.IsEnabled)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      DomainContext.GetEnvironment().ApplyCampagneActiviteit(Environment, campagneactiviteit);
      if (ca.GebruikQueryString()) {
        var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
        BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope);
        var basicRoute = ca.HelperKrijgGoTargetLink(Request.Url.Scheme);
        return RedirectToRoute(RouteUtils.Join(basicRoute, Request.QueryString, param1TakesLead: true));
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Home());
    }

    /// <summary>
    /// Ga naar kantoor
    /// </summary>
    [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
    public RedirectToRouteResult K(string id, string organisatie = null, string c = null) {
      if (!string.IsNullOrEmpty(c))
        DomainContext.GetEnvironment().ApplyCampagneActiviteit(Environment, c);
      Guid locatieID;
      if (Guid.TryParse(id, out locatieID)) {
        var locatie = DomainContext.GetLocaties().GetVoorLinkByInternOrExternId(locatieID);
        if (locatie != null) {
          var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
          BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope, locatie.ID);
          return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(locatie));
        }
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Nietgevonden());
    }

    /// <summary>
    /// Ga naar de reviews van een kantoor
    /// </summary>
    [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
    public RedirectToRouteResult R(string id, string organisatie = null, string c = null) {
      if (!string.IsNullOrEmpty(c))
        DomainContext.GetEnvironment().ApplyCampagneActiviteit(Environment, c);
      Guid locatieID;
      if (Guid.TryParse(id, out locatieID)) {
        var locatie = DomainContext.GetLocaties().GetVoorLinkByInternOrExternId(locatieID);
        if (locatie != null) {
          var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
          BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope, locatie.ID);
          return RedirectToRoute(AdvieskeuzeRoutes.ShowBeoordelingen(locatie));
        }
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Nietgevonden());
    }

    /// <summary>
    /// Ga naar het aanmaken van een adviesgesprek voor een kantoor
    /// </summary>
    [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
    public RedirectToRouteResult A(string id, string productgroep = null, string c = null) {
      if (!string.IsNullOrEmpty(c))
        DomainContext.GetEnvironment().ApplyCampagneActiviteit(Environment, c);
      Guid locatieID;
      if (Guid.TryParse(id, out locatieID)) {
        var locatie = DomainContext.GetLocaties().GetVoorLinkByInternOrExternId(locatieID);
        if (locatie != null) {
          var pg = ProductgroepData.GetProductgroepDB(productgroep);
          var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
          BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope, locatie.ID);
          if (pg == null)
            return RedirectToRoute(AdvieskeuzeRoutes.Adviesgesprek(locatie));
          else
            return RedirectToRoute(AdvieskeuzeRoutes.Adviesgesprek(locatie, pg));
        }
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Nietgevonden());
    }

    /// <summary>
    /// Oude URL's die verwijzen naar widgets afvangen en doorsturen naar banners.
    /// </summary>
    public RedirectToRouteResult Widget(string id, string type, string properties = null) {
      return Banner(id, type, properties, true);
    }

    /// <summary>
    /// Stuur door naar de pagina die de daadwerkelijk banner teruggeeft.
    /// </summary>
    public RedirectToRouteResult Banner(string id, string slug, string properties = null, bool legacyMode = false) {
      Guid entityId;
      EntityType? entityType;
      if (Guid.TryParse(id, out entityId) && slug.IsNotNullOrEmpty() && (entityType = BannerFactory.GetBannerEntityType(slug)).HasValue) {
        switch (entityType) {
          case EntityType.Locatie:
            var locatie = DomainContext.GetLocaties().GetVoorLinkByInternOrExternId(entityId);  // Een flink aantal banners op het net staan met ExternID actief.
            return RedirectToRoute(AdvieskeuzeRoutes.BannerBijNaam(BannerFactory.GetActionFromSlug(slug), properties, locatie, legacyMode));
          case EntityType.Company:
            var companySlug = CompanyData.Get(entityId).Slug;
            return RedirectToRoute(AdvieskeuzeRoutes.BannerBijNaam(BannerFactory.GetActionFromSlug(slug), properties, companySlug, legacyMode));
        }
      }
      return RedirectToRoute(AdvieskeuzeRoutes.Blank());
    }
  }
}