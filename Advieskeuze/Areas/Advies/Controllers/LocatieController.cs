using Advieskeuze.Data;
using Advieskeuze.Data.Domain.ViewModel.Locaties;
using Advieskeuze.Models.Base;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using SharedCode;
using SharedCode.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Advies.Controllers {
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
  public class LocatieController : BaseController {
    [BannedIPCheck]
    public async Task<ActionResult> Details(string postcode, string slug) {
      var model = await DomainContext.GetLocaties().KantoorDetails(postcode, slug);
      if (model.Probleem.HasValue) {
        if (model.Probleem.Value == KantoorDetailModel.OphalenFout.KantoorOnbekend) {
          if (Environment.Productgroep == null)
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
          else
            return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(Environment.Productgroep));
        }
        if (model.Probleem.Value == KantoorDetailModel.OphalenFout.KantoorLinkOud)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowLocatie(model.KantoorLink));
        if (model.Probleem.Value == KantoorDetailModel.OphalenFout.KantoorProductgroepOnbekend) {
          if (Environment.Productgroep == null)
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
          else
            return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(Environment.Productgroep));
        }
      }
      ZetSitemapEnBezoek(model.Kantoor, null, null, AdvieskeuzeRoutes.ShowLocatie(model.KantoorLink, (IProductgroepCache)null));
      return View(model);
    }
    [BannedIPCheck]
    public ActionResult Beoordelingen(string postcode, string slug, int? s, int? beoordelingPageNumber, string scopefilter = null) {
      var model = LocatieModel.CreateBeoordelingLijst(RequestUtil, postcode, slug, s, scopefilter);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowBeoordelingen(model.LocatieSlugStatus.Locatie));
      if (!s.HasValue && model.BeoordelingCount == 0)
        return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(model.Locatie, AdvieskeuzeViewContext.CurrentProductgroep));
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.ShowLocatie(model.Locatie, (Scope)null));
      model.BeoordelingenPagination = model.LocatieBeoordelingen.OrderByDescending(a => a.ZichtbaarVanaf).AsEfficientPagination(beoordelingPageNumber ?? 1, 50);
      return View(model);
    }

    [BannedIPCheck]
    public ActionResult Beoordeling(string postcode, string slug, string slug2) {
      var model = LocatieModel.CreateBeoordeling(RequestUtil, postcode, slug, slug2);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status)) {
        if (model.Locatie == null)
          return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
        if (model.Beoordeling != null)  // Maar als de beoordeling er wel gewoon is, redirect dan naar beoordeling z'n eigen (waarschijnlijk nieuwe) locatie
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowBeoordeling(model.Beoordeling));
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      }
      if (model.Beoordeling == null) {
        return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(model.LocatieSlugStatus.Locatie));
      }
      if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowBeoordeling(slug2, model.LocatieSlugStatus.Locatie));
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, model.Beoordeling.BevestigdDatum.ToLongDateString(), AdvieskeuzeRoutes.ShowBeoordeling(slug2, model.Locatie, (Scope)null));
      return View(model);
    }
    public ActionResult BekijkBeoordeling(string postcode, string slug, string slugbeoordeling) {
      return RedirectToRoute(AdvieskeuzeRoutes.ShowBeoordeling(postcode, slug, slugbeoordeling, AdvieskeuzeViewContext.CurrentProductgroep));
    }

    [BannedIPCheck]
    public ActionResult Tarieven(string postcode, string slug, string slug2) {
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Adviesgesprek(locatieSlugStatus.Locatie));
      var model = new TarievenModel(locatieSlugStatus.Locatie as Locatie);
      model.Scope = ProductgroepData.GetProductgroepDB(slug2);
      if (model.Scope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      model.LocatieScope = model.Locatie.LocatieScope.FirstOrDefault(ls => ls.ScopeID == model.Scope.ID);
      if (model.LocatieScope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      model.Specialisten = AdviseurData.GetSpecialistenZichtbaar(model.Locatie.ID, model.Scope.ID);
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.Tarieven(postcode, slug, (Scope)null, slug2));
      return View(model);
    }

    [SetCampagneActiviteitScope(CampagneActiviteitScope.Aanvragen)]
    [BannedIPCheck]
    public ActionResult Adviesgesprek(string postcode, string slug, string medewerker) {
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Adviesgesprek(locatieSlugStatus.Locatie));
      var model = new AdviesgesprekModel(locatieSlugStatus.Locatie as Locatie, AdvieskeuzeViewContext.CurrentProductgroep, medewerker);
      if (!model.Locatie.HeeftEmailadres)
        return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(model.Locatie));
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.Adviesgesprek(model.Locatie, (Scope)null));
      return View(model);
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Adviesgesprek(string postcode, string slug, string medewerker, FormCollection form) {
      var request = RequestUtil;
      var productgroep = AdvieskeuzeViewContext.CurrentProductgroep;
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(productgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Adviesgesprek(locatieSlugStatus.Locatie));
      var model = new AdviesgesprekModel(locatieSlugStatus.Locatie as Locatie, AdvieskeuzeViewContext.CurrentProductgroep, medewerker);
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.Adviesgesprek(model.Locatie, (Scope)null));
      if (TryUpdateModel(model.Invulformulier, "Invulformulier")) {
        // Max 1 adviesgesprek per persoon per kantoor
        var issue = AdviesgesprekData.GetRate(model.Locatie, model.Invulformulier.Emailadres, model.Invulformulier.Telefoon);
        if (issue != null && issue.LocatieRecentAlGesproken) {
          ModelState.AddModelError(string.Empty, "Je hebt recent al een contactverzoek ingediend bij dit kantoor.");
          return View(model);
        }
        // Max 1 adviesgesprek per persoon per X seconden
        if (issue != null && issue.AlgemeenRecentAlIngediend) {
          ModelState.AddModelError(string.Empty, "Wacht even een paar seconden voordat je een nieuw contactverzoek indient.");
          return View(model);
        }
        // Max X adviesgesprekken per persoon per dag
        if (issue != null && issue.TeVeelVandaag) {
          ModelState.AddModelError(string.Empty, "Je hebt vandaag al een aantal contactverzoeken gedaan. Je kan het beste wachten tot een van de dienstverleners contact met je opneemt.");
          return View(model);
        }
        // Check met de blacklist
        var blacklist = GewrapteSettings.AdviesgesprekBlacklist;
        if (!string.IsNullOrEmpty(blacklist)) {
          foreach (var blacklistItem in blacklist.Split(',')) {
            if (model.Invulformulier.Emailadres.ToLower().Contains(blacklistItem) || model.Invulformulier.Naam.ToLower().Contains(blacklistItem)) {
              ModelState.AddModelError(string.Empty, "Er is iets foutgegaan met het verzoek. Controleer je naam en/of e-mailadres.");
              return View(model);
            }
          }
        }
        AdviesgesprekData.Aanmaken(request.IP, model.Locatie, model.Adviseur,
          model.Invulformulier.Naam, model.Invulformulier.Bedrijfsnaam, model.Invulformulier.Telefoon, model.Invulformulier.Emailadres, model.Invulformulier.ProductgroepSlug, model.Invulformulier.ContactMomentSlug, model.Invulformulier.ToestemmingVervolgTraject,
          CampagneActiviteitContext.Current.CampagneActiviteitTraceID);
        return RedirectToRoute(AdvieskeuzeRoutes.AdviesgesprekBedankt(model.Locatie));
      }
      return View(model);
    }
    public ActionResult AdviesgesprekBedankt(string postcode, string slug) {
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.AdviesgesprekBedankt(locatieSlugStatus.Locatie as Locatie));
      var locatie = locatieSlugStatus.Locatie as Locatie;
      ZetSitemapEnBezoek(locatie, locatie.Foto, null, AdvieskeuzeRoutes.AdviesgesprekBedankt(locatie, (Scope)null));
      return View(locatie);
    }

    [SetCampagneActiviteitScope(CampagneActiviteitScope.Aanvragen)]
    [BannedIPCheck]
    public ActionResult Belmeterug(string postcode, string slug) {
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Belmeterug(locatieSlugStatus.Locatie));
      var model = new BelmeterugModel(locatieSlugStatus.Locatie as Locatie, AdvieskeuzeViewContext.CurrentProductgroep);
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.Belmeterug(model.Locatie, (Scope)null));
      return View(model);
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Belmeterug(string postcode, string slug, FormCollection form) {
      var request = RequestUtil;
      var productgroep = AdvieskeuzeViewContext.CurrentProductgroep;
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(productgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Belmeterug(locatieSlugStatus.Locatie));
      var model = new BelmeterugModel(locatieSlugStatus.Locatie as Locatie, AdvieskeuzeViewContext.CurrentProductgroep);
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.Belmeterug(model.Locatie, (Scope)null));

      if (TryUpdateModel(model.Invulformulier, "Invulformulier")) {
        // Er mag maar 1 per zelfde persoon per dag komen voor n kantoor
        // Voorkomt back-knop problemen (veeeeeelvuldig aanwezig)
        var issue = BelmeterugData.GetRate(model.Locatie, model.Invulformulier.Naam, model.Invulformulier.Telefoon);
        if (issue != null && issue.LocatieRecentAlGesproken) {
          ModelState.AddModelError(string.Empty, "Je hebt recent al een belverzoek ingediend bij dit kantoor");
          return View(model);
        }
        // Rate limiter (1 per 30 sec) als een zelfde persoon per dag meerdere kantoren wil doen
        if (issue != null && issue.AlgemeenRecentAlIngediend) {
          ModelState.AddModelError(string.Empty, "Wacht even een paar seconden voordat je een nieuw belverzoek indient");
          return View(model);
        }
        BelmeterugData.Aanmaken(request.IP, model.Locatie, model.Invulformulier.Naam, model.Invulformulier.Telefoon, CampagneActiviteitContext.Current.CampagneActiviteitTraceID);
        return RedirectToRoute(AdvieskeuzeRoutes.BelmeterugBedankt(model.Locatie));
      }
      return View(model);
    }
    public ActionResult BelmeterugBedankt(string postcode, string slug) {
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (locatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.BelmeterugBedankt(locatieSlugStatus.Locatie as Locatie));
      var locatie = locatieSlugStatus.Locatie as Locatie;
      ZetSitemapEnBezoek(locatie, locatie.Foto, null, AdvieskeuzeRoutes.BelmeterugBedankt(locatie, (Scope)null));
      return View(locatie);
    }

    [BannedIPCheck]
    public ActionResult Betrouwbaarheidsstempel(string postcode, string slug) {
      var model = LocatieModel.CreateDetailsSimpel(RequestUtil, postcode, slug);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status))
        return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.LocatieBetrouwbaarheidsstempel(model.LocatieSlugStatus.Locatie));
      ZetSitemapEnBezoek(model.Locatie, model.Locatie.Foto, null, AdvieskeuzeRoutes.LocatieBetrouwbaarheidsstempel(model.Locatie, (Scope)null));
      return View(model);
    }

    private void ZetSitemapEnBezoek<T>(T locatie, IFotoBestandLink contextImage, string datumDisplay, object route) where T : IKantoorSitemap, IEntityWithID {
      var caID = Environment.HeeftCampagneActiviteit ? Environment.CampagneActiviteit.Data.ID : (Guid?)null;
      BezoekData.Add(RequestUtil, caID, Environment.CampagneActiviteit.Scope, locatie.ID);
      SiteMap.ObjectForFormatting = new {
        locatie = Formatter.RemoveSpecialChars(locatie.Naam),
        plaats = Formatter.RemoveSpecialChars(locatie.Plaats),
        datum = datumDisplay ?? string.Empty
      };
      AdvieskeuzeViewContext.CanonicalUrlRouteValuesSet(RouteUtils.AnonymousObjectToHtmlAttributes(route), HttpContext);
      if (contextImage != null)
        AdvieskeuzeViewContext.OpenGraphAzureImageSet($"{contextImage.ID}{contextImage.Extensie}", HttpContext);
    }
  }
}
