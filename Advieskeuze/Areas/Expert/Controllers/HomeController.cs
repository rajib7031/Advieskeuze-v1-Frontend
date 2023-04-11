using Advieskeuze.Areas.Expert.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert.Controllers {
  /// <summary>
  /// Begin voor consumenten deel expert platform
  /// </summary>
  /// Deze controller is het startpunt van het consumenten deel van het expert platform.
  /// Hier kan je beginnen met zoeken, de experts bekijken en de algemeen zichtbare vragen
  /// bekijken.
  /// 
  /// Hier kan je de publiekelijk beschikbare vragen zien met algemene informatie.
  /// Het is ook mogelijk om meer in detail te gaan wat de vraag antwoorden betreft maar
  /// dat is voorbehouden aan de vraagsteller en dat gebeurt in de StelEenVraag controller.
  /// 
  /// Alle experts zijn hier zichtbaar. Van hun gegeven antwoord historie zijn alleen de
  /// publiekelijk zichtbare antwoorden hier in te zien.
  /// 
  /// De zichtbaarheid van de vraag is afhankelijk van de instellingen van het platform.
  /// <remarks>
  /// Beveiliging is een belangrijk deel van de expert tool. Hoewel we zelf vinden dat
  /// we het meeste hebben aan een publiekelijk zichtbare antwoorden is het platform ook
  /// voor minder publieke discussies gebouwd en in gebruik.
  /// </remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.ExpertPlatform)]
  public class HomeController : Controller {
    public ActionResult Index() {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.Create();
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.ExpertplatformZoeken(zoekTerm));
    }

    public ActionResult VraagDetails(string id) {
      var request = new RequestToken(HttpContext);
      AdvieskeuzeViewContext.CanonicalUrlRouteValuesSet(RouteUtils.AnonymousObjectToHtmlAttributes(AdvieskeuzeRoutes.ExpertVraagDetails(id)), HttpContext);
      var model = HomeModel.CreateVraagDetails(id);
      if (model.Vraag == null)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertHome());
      SitemapTitleFormatter.RegisterObjectForFormatting(new { vraag = model.Vraag.Titel, omschrijving = model.Vraag.Omschrijving.TrimLength(150, "...") });
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }

    public ActionResult Expert(string id, int? page) {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.CreateExpert(id, page);
      if (model.Expert == null)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertHome());
      SitemapTitleFormatter.RegisterObjectForFormatting(new { expert = model.Expert.Naam });
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }
  }
}