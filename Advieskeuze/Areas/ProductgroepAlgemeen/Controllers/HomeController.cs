using Advieskeuze.Data;
using Advieskeuze.Data.Domain.ViewModel.Home;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Common.EntityLinq;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using SharedCode;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Areas.ProductgroepAlgemeen.Controllers {
  /// <summary>
  /// Productgroep landingspagina
  /// </summary>
  /// Deze controller toont een dynamisch gegenereerd beeld van een productgroep.
  /// 
  /// Er zijn verschillende opties beschikbaar voor een productgroep:
  /// -Zoekresultaat
  /// -Beoordelingroute
  /// -Kennisbank
  /// -Expert tool
  /// 
  /// Het zoekresultaat is een zoekmodule die als default gezet is voor een productgroep. Als je de invoer gebruikt
  /// om te zoeken sturen we je door naar t productgroep zoekresultaat 'in'.
  /// Beoordelingsroute is een route die als default gezet is voor een productgroep. Je gaat direct naar de
  /// beoordelingsengine en kunt daar een kantoor en adviseur selecteren.
  /// De kennisbank is een link naar een onderwerp in de kennisbank die als default is gezet voor de productgroep.
  /// De expert tool is niet meer dan een link naar de expert tool.
  /// 
  /// Het is mogelijk om de huidige productgroep af te leiden uit de url. Dat kan door te kijken in welke area
  /// men zit (route) of als er een productgroep slug meegegeven is.
  /// 
  /// Routing is zo gemaakt dat de productgroep slug ipv ProductgroepAlgemeen gebruikt wordt. Dit werkt samen met de
  /// beoordelingsmodule van deze productgroep en met t zoekresultaat.
  /// <remarks>
  /// Home werkt alleen als er iig een zoekresultaat of een beoordelingsmodule is.
  /// </remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class HomeController : Controller {
    public ActionResult Index(string plaats) {
      var model = new HomeFront();
      model.ParticuliereProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeParticuliereProductgroepenLijst(), "Key", "Text");
      model.ZakelijkeProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeZakelijkeProductgroepenLijst(), "Key", "Text");
      var cache = new CacheData(new DataContextFactory(DataContext.Current));
      model.AantalBeoordelingen = cache.GetAantalBeoordelingen();
      model.AantalZoekopdrachten = cache.GetAantalZoekopdrachtenAfgelopenMaand();
      model.AantalLocaties = cache.GetAantalLocaties();
      model.AantalPartnersites = cache.GetAantalPartnersites();
      model.AantalAdviseurs = cache.GetAantalAdviseurs();
      model.Beoordelingen = BeoordelingData.GetZichtbaarFrontend().MetStatus(BeoordelingStatus.Goedgekeurd).Where(b => b.Toelichting.Length >= 140 && b.Naam != "anoniem").OrderByDescending(b => b.ZichtbaarVanaf).Take(2);
      model.Partnersites = PartnerWebsiteData.Get().Where(p => p.IsEtalage).Take(6);
      model.Productgroep = AdvieskeuzeViewContext.CurrentProductgroep;
      if (model.Productgroep == null || !model.Productgroep.HeeftHome)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      model.Productgroep.VoegProductgroepAanSitemapToe(new SitemapTitleFormatter());
      model.PlaatsPrefill = plaats;
      return View(model);
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Index(string locatie, string plaats) {
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(AdvieskeuzeViewContext.CurrentProductgroep, locatie));
    }
    public ActionResult Zoek() {
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.Home());
    }
  }
}
