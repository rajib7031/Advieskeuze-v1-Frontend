using Advieskeuze.Areas.Expert.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.Zoeken;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert.Controllers {
  /// <summary>
  /// Zoeken binnen expert vragen.
  /// </summary>
  /// Je kunt hier zoeken binnen de publiekelijk zichtbare expert vragen. Het idee is dat
  /// je een onderwerp kiest waarna wij binnen de vragen gekoppeld aan het onderwerp gaan
  /// zoeken naar je zoekterm.
  /// 
  /// Het zoeken gebeurd op basis van sleutelwoorden. Alle artikelen hebben sleutelwoorden aan
  /// zich gekoppeld en we gaan via de sleutelwoorden die het meest lijken op de vraag de 
  /// artikelen vinden.
  /// Daarvoor nemen we de zoekterm en leggen deze naast alle beschikbare zoekwoorden en 
  /// bepalen de gelijkenis via de formule van Levenshtein. De meest gelijkende gebruiken we
  /// om te bepalen wat de zoekterm is waarop we zoeken.
  /// We zoeken ook met de 'ruwe' zoekterm in de titel voor directe matches.
  /// 
  /// We bepalen een sleutelwoorden suggestie lijst door de sleutelwoorden die via de formule 
  /// gevonden zijn te combineren met een paar sleutelwoorden van het artikel. Zo kan de 
  /// klant zijn vraag verbeteren door het gebruiken van een specifiek sleutelwoord als 
  /// zoekterm wanneer wij het antwoord niet konden vinden adhv zijn eigen zoekterm.
  /// 
  /// Bij het uitvoeren van een zoekterm doen we een logging op de gebruikte zoekterm.
  /// <remarks></remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.ExpertPlatform)]
  public class ZoekresultaatController : Controller {
    public ActionResult Index(string id, string zoekTerm, ZoekresultaatSorting sorting) {
      var request = new RequestToken(HttpContext);
      AdvieskeuzeViewContext.CanonicalUrlRouteValuesSet(RouteUtils.AnonymousObjectToHtmlAttributes(AdvieskeuzeRoutes.ExpertplatformZoeken(null), null, new string[] { "id", "onderwerp", "zoekterm" }), HttpContext);
      var model = ZoekresultaatModel.Create(id, zoekTerm, sorting);
      model.ZoekresultaatPaged = model.Zoekresultaat.AsEfficientPagination(sorting.Page ?? 1, 10);
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      ZoekenData.Add(request, ZoekresultaatType.Expert, null, zoekTerm, null, CampagneActiviteitContext.Current.GetActieveCampagneActiviteit());
      return View(model);
    }
    [HttpPost]
    public RedirectToRouteResult Index(string onderwerp, string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.ExpertplatformZoeken(onderwerp, zoekTerm));
    }
  }
}
