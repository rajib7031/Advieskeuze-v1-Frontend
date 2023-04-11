using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Advies.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Zoekresultaat)]
  public class ZoekresultaatController : Controller {
    public ActionResult Index() {
      // Dit is een stub voor replacement van oude zoekresultaat functionaliteit
      return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(null));
    }
  }
}
