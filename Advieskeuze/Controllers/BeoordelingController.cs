using Advieskeuze.Data;
using Advieskeuze.Data.Domain;
using Advieskeuze.Data.Domain.ViewModel.Beoordeling;
using Advieskeuze.Models;
using AdvieskeuzeCode;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using AdvieskeuzeCode.Social.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  /// <summary>
  /// Beoordeling bijzaken
  /// </summary>
  /// Deze controller handeld flow zaken rond de beoordeling af waarbij publiekelijke interactie nodig is.
  /// Zoals invullen bedankt, bevestigen e-mailadres, toelichting geven en t kiezen van bedankjes.
  /// 
  /// Bij het indienen van een beoordeling kan een klant ervoor kiezen alsnog zijn e-mailadres te wijzigen.
  /// 
  /// Bedankjes worden aan een klant getoond en verstuurd op basis van een bedankjes systeem. Dit
  /// systeem maakt een koppeling tussen organisaties (via campanges) en beoordelingsmodules. Een
  /// default pakt het over als er niets gevonden wordt.
  /// De bedankjes worden via het organisatie extranet weer gerapporteerd aan de betreffende organsiatie.
  /// <remarks>
  /// De officiele benaming is 'review' en de 'beoordeling' benaming zou er uit moeten gaan. Dat is echter
  /// makkelijker gezegt dan gedaan.
  /// </remarks>
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Beoordeling)]
  public class BeoordelingController : BaseController {
    public new BeoordelingDomain DomainContext => base.DomainContext.GetBeoordeling();

    public ActionResult Index(string postcode, string kantoor, string adviseur = null, bool? isDemo = null, string klantemail = null, string klantnaam = null, int? waardering = null, Guid? companyId = null) {
      var model = HomeModel.CreateBeoordelingen(new RequestToken(HttpContext), postcode, kantoor, adviseur, isDemo, klantemail, klantnaam, waardering);
      if (model.ValideLocatie && model.Locatie != null) {
        model.IngevuldeBedrijfsnaam = model.Locatie.Naam;
        model.IngevuldeLocatieId = model.Locatie.ID;
        model.IngevuldeDienstverlener = adviseur;
        model.IngevuldePlaats = model.Locatie.Plaats;
        if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status))
          return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling());
        if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.Beoordeling(model.LocatieSlugStatus.Locatie));
      }
      else if (model.ValideLocatie && model.Locatie == null) {
        return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling()); // Quick'nDirty naar zichzelf redirecten als er bijv een verwijderde locatie wordt gebruikt.
      }
      if (companyId.HasValue) {
        model.IngevuldeCompanyId = companyId.Value;
      }
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(FormCollection form, string postcode, string kantoor, string adviseur = null, bool? isDemo = null, string klantemail = null, string klantnaam = null, int? waardering = null) {
      var model = HomeModel.CreateBeoordelingen(new RequestToken(HttpContext), postcode, kantoor, adviseur, isDemo, klantemail, klantnaam, waardering);
      if (model.ValideLocatie && model.Locatie != null) {
        if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status))
          return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling());
        if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.Beoordeling(model.LocatieSlugStatus.Locatie));
      }
      if (TryUpdateModel(model)) {
        model.Locatie = model.IngevuldeLocatieId.HasValue ? LocatieData.GetLocatie(model.IngevuldeLocatieId.Value) : null;
        model.AdviseurSlug = model.IngevuldeDienstverlenerId.HasValue ? AdviseurData.GetAdviseur(model.IngevuldeDienstverlenerId.Value).Slug : model.IngevuldeDienstverlener;
        model.Productgroep = String.IsNullOrEmpty(model.IngevuldeProductgroep) ? null : ProductgroepData.GetProductgroep_Cached(model.IngevuldeProductgroep);
        // Zet de tekstuele invoervelden omdat er vrije invoervelden worden geaccepteerd (als er een match is, pak ze van de locatie).
        var link = new KantoorSlugLink();
        link.KantoorPostcode = model.IngevuldePlaats;
        link.KantoorSlug = model.IngevuldeBedrijfsnaam;
        if (model.Locatie != null) {
          link.KantoorPostcode = model.Locatie.Postcode;
          link.KantoorSlug = model.Locatie.Slug;
        }

        var beoordelingmodules = BeoordelingmoduleData.GetBeoordelingmodulesVoorProductgroep(model.Productgroep.ID).ToList();
        if (Request.Form["submit-kort"] != null && beoordelingmodules != null && beoordelingmodules.Any(b => b.Slug.Contains("-kort"))) {
          return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(model.Productgroep, link, model.AdviseurSlug, model.IsDemo, model.BeoordelaarEmail, model.BeoordelaarNaam, true, model.Waardering));
        }
        else {
          return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(model.Productgroep, link, model.AdviseurSlug, model.IsDemo, model.BeoordelaarEmail, model.BeoordelaarNaam, false, model.Waardering));
        }
      }
      return View(model);
    }
    public ActionResult Promotie(string id) {
      ViewData["promotie"] = id;
      return View();
    }
    public ActionResult BestaatFout(Guid id) {
      return View();
    }
    public async System.Threading.Tasks.Task<ActionResult> Toelichten(Guid id) {
      var model = await ToelichtenModel.CreateBeoordeling(id);
      if (model.Beoordeling == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      CampagneActiviteitContext.Current.SetCampagneActiviteitNaBevestigen(model.Beoordeling);
      return View(model);
    }
    [HttpPost]
    public async System.Threading.Tasks.Task<ActionResult> Toelichten(Guid id, FormCollection form) {
      var model = await ToelichtenModel.CreateBeoordeling(id);
      if (model.Beoordeling == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      if (TryUpdateModel(model.Reactie, "Reactie")) {
        var toelichting = new System.Text.StringBuilder("Beoordeling van ").AppendLine(model.Beoordeling.Naam);
        toelichting.Append("Voor ").Append(model.Beoordeling.Beoordelingmodule.EntityType.ToString());
        toelichting.Append(" ").AppendLine(model.Beoordeling.Onderwerp).Append(" : ");
        toelichting.AppendLine();
        toelichting.Append(model.Reactie.Body);
        var kwaliteitID = model.Beoordeling.Company != null ? model.Beoordeling.Company.QualityId : (model.Beoordeling.Locatie != null ? model.Beoordeling.Locatie.KwaliteitID : Guid.Empty);
        var argDict = new Dictionary<string, object>();
        argDict.Add("Opmerking", toelichting.ToString());
        var ip = new RequestToken(HttpContext).IP;
        var emailadressen = new List<string>();
        emailadressen.Add(model.NaarModerator.Emailadres);
        var mailID = await System.Threading.Tasks.Task.Run(() =>
          MandrillEmailHelper.SendTemplateAsync(emailadressen, EmailSender.AdvieskeuzeInfo.Address, EmailSender.AdvieskeuzeInfo.DisplayName,
          argDict, MandrillEmailTemplates.BeoordelingToelichtingDoorKlant, ip, null, kwaliteitId: kwaliteitID
        ));
        model.Beoordeling.StatusCode = (int)BeoordelingStatus.KlantBevestigd;
        BeoordelingData.Update(model.Beoordeling);
        var beoordelingUpdater = new AdvieskeuzeCode.Util.BeoordelingHelper.BeoordelingUpdateData(RequestUtil.IP);
        beoordelingUpdater.Acties(model.Beoordeling.ID).DoorKlantToegelicht(mailID);
        return RedirectToAction("ToelichtenBevestigen");
      }
      return View(model);
    }
    public ActionResult ToelichtenBevestigen() {
      return View();
    }

    public ActionResult Bevestigen(Guid? id) {
      if (!id.HasValue)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      var model = DomainContext.StartBevestigen(id.Value);
      if (!model.MagBevestigingTonen)
        return View("Fout");
      DomainContext.DoeBevestigen(model);
      if (model.IsDemo)
        return View("Demo");
      return View(model);
    }

    [HttpPost]
    public JsonResult Demografie(Guid id, FormCollection form) {
      var model = DomainContext.StartBevestigen(id);
      if (!model.MagBevestigingTonen)
        return Json(new { Bericht = "De review bestaat niet of heeft niet de juiste status." });
      if (model.DemografieData == null)
        return Json(new { Bericht = "Je hebt de vragen al beantwoord." });
      if (!TryUpdateModel(model.DemografieData, "DemografieData"))
        return Json(new { Bericht = "Ingevulde waardes niet juist." });

      DomainContext.DemografieOpslaan(model, model.DemografieData);
      string data;

      // Render a partial view as string for JSON result
      using (var sw = new System.IO.StringWriter()) {
        var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, "Demografie");
        var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
        viewResult.View.Render(viewContext, sw);
        viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
        data = sw.GetStringBuilder().ToString();
      }

      return Json(new { Bericht = data });
    }

    [HttpPost]
    public JsonResult VerstuurBedankje(Guid id, BeoordelingBevestigen.Bedankje validatedModel) {
      var model = DomainContext.StartBevestigen(id);
      if (!model.MagBevestigingTonen)
        return Json(new { Bericht = "De review bestaat niet of heeft niet de juiste status." });
      if (!ModelState.IsValid)
        return Json(new { Bericht = "Ingevulde waardes niet juist." });

      DomainContext.BedankjeKiezen(model, validatedModel);
      return Json(new { Bericht = "Het bedankje is per e-mail naar je verstuurd." });
    }
  }
}
