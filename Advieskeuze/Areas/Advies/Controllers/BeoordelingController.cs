using Advieskeuze.Data;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Advies.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Beoordeling)]
  public class BeoordelingController : Controller {
    public ActionResult Index(string postcode, string kantoor) {
      // Stub, deprecated. Deze wordt echter nog vaak gerefereerd
      var locatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(new RequestToken(HttpContext), postcode, kantoor);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(locatieSlugStatus.Status))
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.Beoordeling(locatieSlugStatus.Locatie));
      return RedirectToRoutePermanent(AdvieskeuzeRoutes.Beoordeling());
    }
  }
}