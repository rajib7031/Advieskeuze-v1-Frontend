using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class ParticulierController : BaseController {
    //[OutputCache(CacheProfile = "DynamicPage")]
    public ActionResult Index() {
      var model = DomainContext.GetHome().GetParticulier();
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(string locatie, string homeproductgroep) {
      var model = DomainContext.GetHome().GetFrontRedirect(locatie, homeproductgroep);
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(model.Productgroep, model.Zoektekst));
    }
  }
}
