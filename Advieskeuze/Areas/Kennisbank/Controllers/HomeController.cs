using Advieskeuze.Areas.Kennisbank.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Kennisbank.Controllers {
  /// <summary>
  /// Kennisbank artikelen startpunt en toon mechanisme.
  /// </summary>
  /// De kennisbank bestaat uit artikelen die door de moderators van Advieskeuze.nl
  /// gemaakt zijn. De kennis komt uit online en offline beschikbare bronnen.
  /// 
  /// Een artikel is een mooi opgemaakt stuk. Voor de opmaak wordt de bb-editor gebruikt.
  /// 
  /// Bij een artikel worden gerelateerde artikelen getoond. Dat wordt bepaald doordat
  /// een artikel via sleutelwoorden aan ander artikelen is gekoppeld. De gerelateerde
  /// artikelen worden per sleutelwoord via de formule van Levenshtein vergeleken met
  /// de sleutelwoorden van het getoonde artikel. Dichtbij liggende sleutelwoorden (en
  /// dus artikelen) worden getoond.
  /// 
  /// De categorien zijn bedoeld om de artikelen binnen een context te plaatsen.
  /// Categorien zijn rechtstreeks gekoppeld aan de artikelen. Een categorie is 
  /// verplicht. De onderlinge relatie tussen artikelen wordt niet belemmerd door de
  /// categorie grens.
  /// 
  /// De categorie heeft een productgroep gekoppeld. Vanuit de productgroep home pagina
  /// kan zo direct naar de betreffende kennisbank pagina verwezen worden.
  /// 
  /// Een eerdere versie had de mogelijkheid om commentaar te geven op de artikelen.
  /// Ook was er het idee dat adviseurs zelf artikelen konden maken.
  /// Dat is los gelaten ten favore van het expert platform wat nu die functionaliteit
  /// deels invult.
  /// <remarks></remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class HomeController : Controller {
    public ActionResult Index() {
      return View(KennisbankModel.Create());
    }
    [HttpPost]
    public ActionResult Index(string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.KennisbankZoeken(zoekTerm));
    }

    public ActionResult Categorie(string id) {
      var model = CategorieModel.Create(id);
      if (model.Categorie == null)
        return RedirectToRoute(AdvieskeuzeRoutes.KennisbankHome());
      SetMetaData(AdvieskeuzeRoutes.KennisbankCategorie(id), id, id);
      return View(model);
    }
    [HttpPost]
    public ActionResult Categorie(string id, string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.KennisbankZoeken(zoekTerm));
    }

    public ActionResult Artikel(string id, string sleutelwoord) {
      var model = ArtikelModel.Create(id, sleutelwoord);
      if (model.Artikel == null || !model.Artikel.IsActief)
        return RedirectToRoute(AdvieskeuzeRoutes.KennisbankHome());
      SetMetaData(AdvieskeuzeRoutes.Artikel(id), model.Artikel.Titel, model.Artikel.Samenvatting);
      return View(model);
    }
    [HttpPost]
    public ActionResult Artikel(string zoekTerm) {
      return RedirectToRoute(AdvieskeuzeRoutes.KennisbankZoeken(zoekTerm));
    }

    private void SetMetaData(object route, string title, string content) {
      title = Formatter.ReplaceSpecialChars(title);
      SitemapTitleFormatter.RegisterObjectForFormatting(new { title, content });
    }
  }
}