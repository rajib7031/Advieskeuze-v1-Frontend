using Advieskeuze.Areas.Expert.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert.Controllers {
  /// <summary>
  /// Onderwerp overzicht 
  /// </summary>
  /// De onderwerpen zijn bedoeld om de expert vragen binnen een context te plaatsen.
  /// 
  /// Onderwerpen zijn rechtstreeks gekoppeld aan expert vragen. Een vraag steller moet
  /// aan het begin van zijn route een onderwerp opgeven of deze is al voor hem
  /// vooringevuld. Een onderwerp is verplicht.
  /// <remarks>
  /// Het onderwerp is een context waarbinnen een vraag is gesteld. Dit onderwerp zal altijd
  /// gebruikt worden, bijvoorbeeld ook bij zoeken.
  /// </remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.ExpertPlatform)]
  public class OnderwerpController : Controller {
    public ActionResult Index(string id) {
      var request = new RequestToken(HttpContext);
      var model = HomeModel.CreateOnderwerp(id);
      BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope);
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(string onderwerp, string zoekTerm, FormCollection form) {
      return RedirectToRoute(AdvieskeuzeRoutes.ExpertplatformZoeken(onderwerp, zoekTerm));
    }

  }
}