using Advieskeuze.Areas.StedenAlgemeen.Models;
using Advieskeuze.Data;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using AdvieskeuzeCode.Searchengine;
using System.Web.Mvc;

namespace Advieskeuze.Areas.StedenAlgemeen.Controllers {
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class HomeController : BaseController {
    private string _stadSlug => RouteData.Values["stadslug"].ToString();
    public ActionResult Index() {
      var model = StadModel.Create(_stadSlug);
      SiteMap.ObjectForFormatting = new {
        plaats = model.Plaats.Naam
      };
      return View(model);
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Index(string productgroep, SortType sortType) {
      var pg = ProductgroepData.GetProductgroep_Cached(productgroep);
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(pg, _stadSlug, (int)sortType));
    }
  }
}
