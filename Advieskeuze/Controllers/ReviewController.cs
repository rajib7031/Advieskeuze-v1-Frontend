using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Beoordeling)]
  public class ReviewController : BaseController {
    /// <summary>
    /// De /beoordeling moet gaan redirecten naar /review.
    /// Voor nu nog even andersom zodat marketing de /review URL al kan communiceren.
    /// </summary>
    public ActionResult Index() {
      return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling());
    }
  }
}