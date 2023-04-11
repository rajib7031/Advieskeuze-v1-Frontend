using Advieskeuze.Data;
using Advieskeuze.Models;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Models;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using AdvieskeuzeCode.Social.Emails;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class HomeController : BaseController {
    public ActionResult Index(string plaats) {
      var model = DomainContext.GetHome().GetFront();
      model.PlaatsPrefill = plaats; // Wordt nu alleen gebruikt om vanaf vrije kantoren de kantoorplaats mee te geven bij het zoeken naar een ander bedrijf.
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(string locatie, string homeproductgroep) {
      var model = DomainContext.GetHome().GetFrontRedirect(locatie, homeproductgroep);
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(model.Productgroep, model.Zoektekst));
    }
    /// <summary>
    /// Wordt gebruikt in Iframes op consumentenbond.nl
    /// </summary>
    public ActionResult Zoeken() {
      var model = DomainContext.GetHome().GetFront();
      return View(model);
    }
    [HttpPost]
    public ActionResult Zoeken(string locatie, string homeproductgroep) {
      var model = DomainContext.GetHome().GetFrontRedirect(locatie, homeproductgroep);
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(model.Productgroep, model.Zoektekst));
    }
    public ActionResult Privacy() {
      return View();
    }
    public ActionResult Cookiebeleid() {
      return View();
    }
    public ActionResult Kassa() {
      return RedirectToRoute(AdvieskeuzeRoutes.Home());
    }
    public ActionResult Sitemap() {
      var model = DomainContext.GetHome().GetSitemap();
      return View(model);
    }
    public ActionResult Vacatures() {
      var faqCategorie = FaqData.GetCategorie("vacatures");
      var faqCms = FaqData.GetVragen(faqCategorie).ToList();
      return View(faqCms);
    }
    public ActionResult Disclaimer() {
      return View();
    }
    public ActionResult Kernwaarden() {
      return View();
    }
    public ActionResult VoorwaardenBeoordeling() {
      return View();
    }
    public ActionResult Reviewbeleid() {
      return View();
    }
    public ActionResult SpelregelsBeoordeling() {
      return View();
    }
    public ActionResult PartnerWebsites() {
      var model = DomainContext.GetHome().GetPartnerWebsites();
      return View(model);
    }
    public ActionResult Contact() {
      return View(new EmailComposeModel());
    }
    public ActionResult NietGevonden() {
      return View();
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Contact(EmailComposeModel email) {
      if (ModelState.IsValid) {
        var argDict = new Dictionary<string, object>();
        argDict.Add("KlantNaam", email.Van.Replace("@", "(@)").Replace(".", "(.)"));
        argDict.Add("Opmerking", email.Body);
        List<string> emailadressen = new List<string>();
        emailadressen.Add(EmailSender.AdvieskeuzeInfo.Address);
        System.Threading.Tasks.Task.Run(() =>
          MandrillEmailHelper.SendTemplateAsync(emailadressen, EmailSender.AdvieskeuzeInfo.Address, EmailSender.AdvieskeuzeInfo.DisplayName,
          argDict, MandrillEmailTemplates.ExtranetContactformulier, RequestUtil.IP, "Bezoeker vult contactformulier in"
        ));

        return RedirectToRoute(AdvieskeuzeRoutes.ContactBedankt());
      }
      return View(email);
    }
    public ActionResult ContactBedankt() {
      return View();
    }
    public ActionResult Beoordeling() {
      return View();
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Beoordeling(FormCollection form) {
      var score = form.GetValues("score")[0];
      var toelichting = form.GetValues("toelichting")[0];
      if (string.IsNullOrEmpty(score) || string.IsNullOrEmpty(toelichting))
        return View();

      var argDict = new Dictionary<string, object>();
      argDict.Add("Score", score);
      argDict.Add("Toelichting", toelichting);
      argDict.Add("IP", RequestUtil.IP.ToMD5());
      var emailadressen = new List<string>();
      emailadressen.Add(EmailSender.AdvieskeuzeInfo.Address);
      System.Threading.Tasks.Task.Run(() =>
      MandrillEmailHelper.SendTemplateAsync(emailadressen, EmailSender.AdvieskeuzeInfo.Address, EmailSender.AdvieskeuzeInfo.DisplayName,
        argDict, MandrillEmailTemplates.AdvieskeuzeBeoordeling, RequestUtil.IP, "Bezoeker vult beoordelingformulier in"
      ));
      return RedirectToRoute(AdvieskeuzeRoutes.AdvieskeuzeBeoordelingBedankt());
    }
    public ActionResult BeoordelingBedankt() {
      return View();
    }
    public ActionResult OverOns() {
      var faqCategorie = FaqData.GetCategorie("overons");
      var faqCms = FaqData.GetVragen(faqCategorie).ToList();
      return View(faqCms);
    }
    //[OutputCache(CacheProfile = "StaticPage")]
    public ActionResult Faq() {
      var model = new FaqIndexModel();
      model.FaqVragen = new List<FaqVraag>() {
        FaqData.GetVraag(Guid.Parse("25263bbc-0ea3-4116-a331-d00e9b543697")),
        FaqData.GetVraag(Guid.Parse("8055b320-5014-4419-a0be-8430d6f88d3c")),
        FaqData.GetVraag(Guid.Parse("25ba09d5-51bb-43cb-88ef-0a90a6e5ba11")),
        FaqData.GetVraag(Guid.Parse("ad996ea4-89fb-4ab4-a2f2-e050181a4a76"))
      };
      return View(model);
    }
    //[OutputCache(CacheProfile = "StaticPage")]
    public ActionResult FaqOverzicht() {
      var model = DomainContext.GetHome().GetFaqIndex();
      return View(model);
    }
    //[OutputCache(CacheProfile = "StaticPage")]
    public ActionResult FaqVraag(Guid? id) {
      var model = DomainContext.GetHome().GetFaqVraag(id);
      if (!model.Gevonden)
        return RedirectToRoute(AdvieskeuzeRoutes.Faq());
      SiteMap.ObjectForFormatting = new { vraag = Formatter.RemoveSpecialChars(model.Titel) };
      return View(model);
    }
    //[OutputCache(CacheProfile = "StaticPage")]
    //[CampagneActiviteitCacheWarning]
    public ActionResult Blank() {
      return View();
    }

    public void UitklikLog(string id, int type, Guid? caID) {
      UitklikLogData.Add(RequestUtil, Guid.Parse(id), type, caID);
    }

    public string GetPostcode(decimal lat, decimal lng) {
      var postcode = PostcodeData.GetClosest(lat, lng);
      if (postcode != null)
        return postcode.Postcode1;
      return string.Empty;
    }
  }
}
