using Advieskeuze.Areas.Kennisbank.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.Zoeken;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Kennisbank.Controllers {
  /// <summary>
  /// Zoeken binnen de kennisbank.
  /// </summary>
  /// Je kunt hier zoeken binnen de kennisbank. Je ziet meteen interessante artikelen.
  /// 
  /// Het zoeken gebeurd op basis van sleutelwoorden. Alle artikelen hebben sleutelwoorden aan
  /// zich gekoppeld en we gaan via de sleutelwoorden die het meest lijken op de vraag de 
  /// artikelen vinden.
  /// Daarvoor nemen we de zoekterm en leggen deze naast alle beschikbare zoekwoorden en 
  /// bepalen de gelijkenis via de formule van Levenshtein. De meest gelijkende gebruiken we
  /// om te bepalen wat de zoekterm is waarop we zoeken.
  /// We zoeken ook met de 'ruwe' zoekterm in de titel voor directe matches.
  /// 
  /// We tonen alleen artikelen die vrij zijn gegeven door naar IsActief te kijken.
  /// 
  /// Bij het uitvoeren van een zoekterm doen we een logging op de gebruikte zoekterm.
  /// <remarks></remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class ZoekresultaatController : Controller {
    public ActionResult Index(string zoekTerm, ZoekresultaatSorting sorting) {
      var request = new RequestToken(HttpContext);
      var model = ZoekresultaatModel.Create(zoekTerm, sorting);
      ZoekenData.Add(request, ZoekresultaatType.Kennisbank, null, zoekTerm, null, CampagneActiviteitContext.Current.GetActieveCampagneActiviteit());
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.KennisbankZoeken(zoekTerm));
    }
  }
}
