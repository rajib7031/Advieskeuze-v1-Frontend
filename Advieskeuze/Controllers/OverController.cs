using Advieskeuze.Data;
using Advieskeuze.Data.Domain;
using Advieskeuze.Models;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class OverController : BaseController {
    public new OverDomain DomainContext => base.DomainContext.GetOver();

    public ActionResult Index() {
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.Home());
    }

    public ActionResult Aanbieders() {
      var model = CompanyData.GetByUniverseGroup(UniverseData.SlugAanbieder).ToList();
      return View(model);
    }
    public ActionResult Aanbieder(string id) {
      var model = CompanyData.GetByUniverseGroup(UniverseData.SlugAanbieder, id);
      if (model == null)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverAanbieders());
      SiteMap.ObjectForFormatting = new { aanbieder = model.Name };
      return View(model);
    }
    public ActionResult Ketens() {
      var model = CompanyData.GetByUniverseGroup(UniverseData.SlugKeten).ToList();
      return View(model);
    }
    public ActionResult Keten(string id) {
      var model = new OverKetenModel();
      model.Company = CompanyData.GetByUniverseGroup(UniverseData.SlugKeten, id);
      if (model.Company == null)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverKetens());
      SiteMap.ObjectForFormatting = new { keten = model.Company.Name };
      model.Locaties = LocatieData.GetZoekresultaatLocaties(model.Company).OrderBy(l => l.Naam);
      var average = model.Locaties.Where(k => k.Score != null).Average(k => k.Score);
      if (average.HasValue)
        model.KantorenScore = Math.Round(average.Value, 1);
      if (model.KantorenScore.HasValue) {
        model.KantorenScoreDisplay = model.KantorenScore == 10 ? "10" : model.KantorenScore.Value.ToString(CultureInfo.GetCultureInfo("en-US"));
        model.AantalTotaal = BeoordelingData.CompanyReviews(model.Company).Count();
      }
      // Check if there is a zoekresultaat to be found
      if (!string.IsNullOrEmpty(model.Company.Relation) && model.Company.Organisatie != null) {
        var campagne = CampagneData.GetCampagne("advieskeuze-ketens");
        if (campagne == null)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverKetens());
        var campagnes = CampagneActiviteitData.GetCampagneActiviteiten(campagne).ToList();
        if (campagnes == null)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverKetens());
        var ca = campagnes.FirstOrDefault(c => c.Zoekmodule.BasisKantoorselectieCode == (int)ZoekmoduleBasisKantoorselectie.OrganisatieKetens && c.ZoekmoduleDB.KantoorselectieOrganisatieID.HasValue && c.ZoekmoduleDB.KantoorselectieOrganisatieID.Value == model.Company.OrganisatieID);
        if (ca != null)
          model.ZoekresultaatCampagne = ca;
      }
      model.Beoordelingen = BeoordelingData.CompanyReviews(model.Company).OrderByDescending(b => b.ZichtbaarVanaf).Take(5);
      return View(model);
    }

    [HttpPost]
    public ActionResult Keten(string id, string zoek, FormCollection form) {
      var model = new OverKetenModel();
      model.Company = CompanyData.GetByUniverseGroup(UniverseData.SlugKeten, id);
      if (model.Company == null)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverKetens());
      if (!string.IsNullOrEmpty(model.Company.Relation) && model.Company.Organisatie != null) {
        var campagne = CampagneData.GetCampagne("advieskeuze-ketens");
        var campagnes = CampagneActiviteitData.GetCampagneActiviteiten(campagne).ToList();
        var ca = campagnes.FirstOrDefault(c => c.Zoekmodule.BasisKantoorselectieCode == (int)ZoekmoduleBasisKantoorselectie.OrganisatieKetens && c.ZoekmoduleDB.KantoorselectieOrganisatieID.HasValue && c.ZoekmoduleDB.KantoorselectieOrganisatieID.Value == model.Company.OrganisatieID);
        if (ca != null) {
          if (string.IsNullOrEmpty(zoek))
            return RedirectToRoute(AdvieskeuzeRoutes.GoC(ca.ExternID));
          else
            return RedirectToRoute(AdvieskeuzeRoutes.GoC(ca.ExternID, zoek));
        }
      }
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverKeten(id));
    }

    // Redirects
    public ActionResult Keurmerken() {
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverRegisters());
    }
    public ActionResult Keurmerk(string id) {
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.OverRegister(id));
    }

    public ActionResult Registers() {
      var model = KwaliteitcontroleData.Get().ToList();
      return View(model);
    }
    public ActionResult Register(string id) {
      var register = KwaliteitcontroleData.Get(id);
      if (register == null)
        return RedirectToRoute(AdvieskeuzeRoutes.OverRegisters());
      SiteMap.ObjectForFormatting = new { keurmerk = register.Naam };
      var model = new OverRegisterModel();
      model.Register = register;
      if (register.EntityType == (int)EntityType.Locatie)
        model.AantalEntiteiten = register.LocatieKwaliteitcontrole.Count(lk => lk.IsGoedgekeurd);
      else
        model.AantalEntiteiten = register.AdviseurKwaliteitcontrole.Count(lk => lk.IsGoedgekeurd);
      model.Zoekresultaten = register.KwaliteitcontroleScope.Select(k => k.Scope);
      return View(model);
    }

    public async System.Threading.Tasks.Task<ActionResult> Nederland(string productgroep, string provincie, string gemeente, string plaats) {
      var model = await DomainContext.GetRegio(productgroep, provincie, gemeente, plaats);
      if (model.RedirectType.HasValue) {
        if (model.RedirectType.Value == Data.Domain.ViewModel.Over.OverRegio.RegioType.Gemeente)
          return RedirectToRoute(AdvieskeuzeRoutes.OverRegio(model.Productgroep, provincie, gemeente, null));
        if (model.RedirectType.Value == Data.Domain.ViewModel.Over.OverRegio.RegioType.Provincie)
          return RedirectToRoute(AdvieskeuzeRoutes.OverRegio(model.Productgroep, provincie, null, null));
        if (model.RedirectType.Value == Data.Domain.ViewModel.Over.OverRegio.RegioType.Landelijk)
          return RedirectToRoute(AdvieskeuzeRoutes.OverRegio(model.Productgroep, null, null, null));
        return RedirectToRoute(AdvieskeuzeRoutes.Sitemap());
      }
      SiteMap.ObjectForFormatting = new {
        titel = $"Vergelijk {model.Productgroep.AdviseurNaamMeervoud} in {model.Data.RegioOmschrijving}",
        omschrijving = $"Zoek een {model.Productgroep.AdviseurNaam} in {model.Data.RegioOmschrijving} en bekijk alle tarieven, medewerkers, reviews en openingstijden van het kantoor",
      };
      return View(model);
    }

    internal ActionResult DefaultAction<T>(T model, Func<T, object> sitemapFormatter, Func<T, object> routeOnNoDataFound) where T : IDefaultValidate {
      if (!model.DataFound)
        return RedirectToRoute(routeOnNoDataFound(model));
      if (sitemapFormatter != null)
        SiteMap.ObjectForFormatting = sitemapFormatter(model);
      return View(model);
    }
    public interface IDefaultValidate {
      bool DataFound { get; }
    }
  }
}