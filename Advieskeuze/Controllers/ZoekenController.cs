using Advieskeuze.Data;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Searchengine.Filters;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security.AntiXss;

namespace Advieskeuze.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class ZoekenController : Controller {
    public ActionResult Uitleg(string productgroep) {
      var model = ProductgroepData.GetProductgroep_Cached(productgroep);
      return View(model);
    }
    public JsonResult EncryptFilter(string id) {
      if (string.IsNullOrEmpty(id))
        return Json(string.Empty, JsonRequestBehavior.AllowGet);
      return Json(new FilterEncoder().EncryptToUrl(id), JsonRequestBehavior.AllowGet);
    }
    [BannedIPCheck]
    public ActionResult GetPlaatsForAutoCompletion(string term) {
      var plaatsen = PlaatsData.GetPlaatsForAutoCompletion(term)
        .Take(20).ToList()
        .OrderBy(p => p.Naam)
        .Select(p => p.Naam);
      return Json(plaatsen.ToArray(), JsonRequestBehavior.AllowGet);
    }
    [BannedIPCheck]
    public ActionResult GetLocatiesForAutoCompletion(string term, string plaats, Guid? productgroepID, Guid? companyId) {
      var data = LocatieData.GetLocatiesForAutoCompletion(term, plaats: plaats, companyId: companyId, productgroepID: productgroepID)
        .Take(20).ToList()
        .Select(l => new {
          value = l.InternID,
          label = AntiXssEncoder.HtmlEncode(l.Naam, true),
          desc = AntiXssEncoder.HtmlEncode($"{l.Postcode} {l.Huisnummer}{l.HuisnummerExt}, {l.Plaats}", true),
          fotoID = l.AfbeeldingId
        });
      return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
    }
    [BannedIPCheck]
    public ActionResult GetLocatiesForAutoCompletionSlug(string term, string plaats, string productgroepSlug, Guid? companyId) {
      var data = LocatieData.GetLocatiesForAutoCompletionSlug(term, plaats: plaats, companyId: companyId, productgroepSlug: productgroepSlug)
        .Take(20).ToList()
        .Select(l => new {
          value = l.InternID,
          label = AntiXssEncoder.HtmlEncode(l.Naam, true),
          desc = AntiXssEncoder.HtmlEncode($"{l.Postcode} {l.Huisnummer}{l.HuisnummerExt}, {l.Plaats}", true),
          fotoID = l.AfbeeldingId
        });
      return Json(data.ToArray(), JsonRequestBehavior.AllowGet);
    }
    [BannedIPCheck]
    public ActionResult GetAdviseursForAutoCompletion(string term, string plaats, Guid? productgroepID, Guid? locatieID) {
      Guid? locatieIDParsed = LocatieData.ControleerInternID(locatieID);
      var adviseurs = AdviseurData.GetAdviseursForAutoCompletion(term, plaats: plaats, locatieId: locatieIDParsed, productgroepID: productgroepID);
      return Json(adviseurs, JsonRequestBehavior.AllowGet);
    }
    [BannedIPCheck]
    public ActionResult GetAdviseursForAutoCompletionSlug(string term, string plaats, string productgroepSlug, Guid? locatieID) {
      Guid? locatieIDParsed = LocatieData.ControleerInternID(locatieID);
      var adviseurs = AdviseurData.GetAdviseursForAutoCompletionSlug(term, plaats: plaats, locatieId: locatieIDParsed, productgroepSlug: productgroepSlug);
      return Json(adviseurs, JsonRequestBehavior.AllowGet);
    }
  }
}
