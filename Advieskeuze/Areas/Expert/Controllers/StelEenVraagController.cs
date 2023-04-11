using Advieskeuze.Areas.Expert.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using AdvieskeuzeCode.Social.Emails;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert.Controllers {
  /// <summary>
  /// Algemene expert vraag afhandeling
  /// </summary>
  /// Deze controller handeld de interactie met de klant af. 
  /// 
  /// Een expert vraag heeft de volgende status
  /// -Ingediend (na het aanmaken)
  /// -Bevestigd (door klant is via de link in de e-mail de beoordeling bevestigd)
  /// -Gepubliceerd (door de moderator vrijgegeven om beantwoord te worden door de experts)
  /// -Afgekeurd (door de moderator afgekeurd)
  /// -Beantwoord (binnen de tijdstermijn door voldoende experts beantwoord)
  /// -Onbeantwoord (niet binnen de tijdstermijn door experts beantwoord)
  /// -AntwoordBekeken (de antwoorden zijn door de klant bekeken)
  /// 
  /// Een klant kan een vraag indienen voor een onderwerp. Deze vraag wordt dan eerst gemodereerd
  /// in het expertmoderatie platform. Daarna wordt deze aangeboden aan een netwerk van experts.
  /// Als er een maximum aantal antwoorden of een maximale tijd er overheen is gegaan wordt de
  /// vraag gesloten. Indien er niemand gereageerd had was de vraag onbeantwoord en wordt de klant
  /// op de hoogte gesteld. Indien er 1 of meerdere antwoorden waren dan kan de klant de antwoorden
  /// een tijdje bekijken met contact informatie over de experts. De expert kan daarna zien dat
  /// de klant ook werkelijk zijn antwoord bekeken heeft.
  /// 
  /// Alle interactie die hier plaats vindt is afgeschermd dmv tokens die maar beperkt 
  /// houdbaar zijn. Zo kunnen hier ook de expert vragen afgehandeld worden die niet 
  /// publiekelijk zichtbaar mogen zijn.
  /// 
  /// Er is een versnelde route gemaakt om een onbeantwoorde vraag weer in te dienen.
  /// 
  /// Het is mogelijk om bevestigde vragen direct te publiceren naar de experts. Dat is in te 
  /// stellen op het 'platform'.
  /// 
  /// Een expert is gekoppeld aan een kantoor. 
  /// Voor de klant is dat niet zichtbaar en werkt deze expert bij 'een organisatie' met de naam 
  /// van de betreffende gekoppelde entiteit.
  /// 
  /// Antwoorden zijn maar een tijdje te bekijken in de uitgebreide versie. Daarna zal je,
  /// als je toch een 'oud' linkje hebt, automatisch bij de normale 'zichtbare' versie uitkomen.
  /// <remarks></remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.ExpertPlatform)]
  public class StelEenVraagController : Controller {
    public ActionResult Index() {
      var model = VraagModel.Create(1);
      return View(model);
    }
    [HoneypotCaptcha("FirstName")]
    [HttpPost]
    public ActionResult Index(FormCollection form) {
      var request = new RequestToken(HttpContext);
      var paginaNr = Convert.ToInt32(form.GetValue("PaginaNr").AttemptedValue);
      var model = VraagModel.Create(paginaNr);
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      if (TryUpdateModel(model.Pagina1, "Pagina1")) {
        if (paginaNr == 2 && TryUpdateModel(model.Pagina2, "Pagina2")) {
          try {
            var activiteit = CampagneActiviteitContext.Current.TryGetActieveCampagneActiviteitOrFallbackAdvieskeuze();
            var expertvraag = ExpertVraagData.CreateExpertVraag(
              model.Pagina2.Onderwerp,
              model.Pagina1.Naam,
              model.Pagina1.Emailadres,
              model.Pagina2.Vraag,
              activiteit);
            var argDict = new Dictionary<string, object>();
            argDict.Add("AanhefNaam", expertvraag.Naam);
            argDict.Add("BevestigLink", expertvraag.HasBevestigUrl ? expertvraag.BevestigUrl : null);
            var ip = request.IP;
            List<string> emailadressen = new List<string>();
            emailadressen.Add(expertvraag.Emailadres);
            System.Threading.Tasks.Task.Run(() =>
              MandrillEmailHelper.SendTemplateAsync(emailadressen, EmailSender.AdvieskeuzeInfo.Address, EmailSender.AdvieskeuzeInfo.DisplayName,
              argDict, MandrillEmailTemplates.ExpertVraagBevestigingLink, ip, "Vraag door klant ingediend", kwaliteitId: expertvraag.KwaliteitID, actionType: ActieType.Toegevoegd
            ));
            return RedirectToRoute(AdvieskeuzeRoutes.ExpertStelEenVraagBedankt());
          }
          catch {
            ModelState.AddModelError("", "Er is iets fout gegaan.");
          }
        }
        model.PaginaNr = 2;
        return View(model);
      }
      ModelState.AddModelError("", "Er is iets fout gegaan.");
      return View(model);
    }
    public ActionResult Bedankt() {
      return View();
    }

    public ActionResult Bevestigen(Guid id) {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.CreateAntwoordenBekijken(id);
      if (model.Vraag != null) {
        CampagneActiviteitContext.Current.SetCampagneActiviteitNaBevestigen(model.Vraag);
        if (model.Vraag.Status == ExpertVraagStatus.Ingediend) {
          model.Vraag.Status = ExpertVraagStatus.Bevestigd;
          ExpertVraagData.Update(model.Vraag);
          ActieData.AddSave(request.IP, "Vraag door klant bevestigd", kwaliteitId: model.Vraag.KwaliteitID, type: ActieType.Bevestigd);
        }
      }
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }


    public ActionResult AntwoordenBekijken(Guid? id) {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.CreateAntwoordenBekijken(id);
      if (model.Vraag == null)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertHome());
      CampagneActiviteitContext.Current.SetCampagneActiviteitNaBevestigen(model.Vraag);
      if (!model.Vraag.KanVraagstellerAntwoordBekijken)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertVraagDetails(model.Vraag.Slug));
      if (model.Vraag.Status == ExpertVraagStatus.Beantwoord) {  // We moeten loggen
        model.Vraag.Status = ExpertVraagStatus.AntwoordBekeken;
        model.Vraag.VraagStatusDatum = DateTime.Now.Local();
        ExpertVraagData.Update(model.Vraag);
        ActieData.AddSave(request.IP, "Vraag bekeken door vraagsteller", kwaliteitId: model.Vraag.KwaliteitID, type: ActieType.Bekeken);
      }
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }
    public ActionResult AntwoordDetails(Guid? id, Guid? antwoordID) {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.CreateAntwoordDetailsBekijken(id, antwoordID);
      if (model.Vraag == null)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertHome());
      if (!model.Vraag.KanVraagstellerAntwoordBekijken)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertVraagDetails(model.Vraag.Slug));
      if (model.Antwoord.Status == ExpertAntwoordStatus.Beantwoord) {
        model.Antwoord.Status = ExpertAntwoordStatus.AntwoordBekeken;
        model.LocatieAdviseurs = model.Antwoord.Adviseur.LocatieAdviseur.Where(la => la.IsZichtbaar);
        ExpertAntwoordData.Update(model.Antwoord);
      }
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }

    public ActionResult Onbeantwoord(Guid? id) {
      var model = HomeModel.CreateAntwoordenBekijken(id);
      if (model.Vraag == null)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertHome());
      CampagneActiviteitContext.Current.SetCampagneActiviteitNaBevestigen(model.Vraag);
      if (!model.Vraag.KanVraagstellerOnbeantwoordBekijken)
        return RedirectToRoute(AdvieskeuzeRoutes.ExpertVraagDetails(model.Vraag.Slug));
      return View(model);
    }
  }
}