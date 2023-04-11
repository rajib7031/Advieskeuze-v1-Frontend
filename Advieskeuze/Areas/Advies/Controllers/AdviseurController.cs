using Advieskeuze.Data;
using Advieskeuze.Models.Base;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode;
using SharedCode.Mvc;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Advies.Controllers {
  /// <summary>
  /// De kantoor adviseur controller.
  /// </summary>
  /// Toont een individuele adviseur.
  /// Zie de kantoordetail pagina voor uitleg over de gebruikte mechanismen.
  /// <remarks></remarks>
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
  public class AdviseurController : Controller {
    public ActionResult Details(string postcode, string slugLocatie, string slugAdviseur) {
      var model = LocatieAdviseurModel.Create(postcode, slugLocatie, slugAdviseur);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status)) {
        if (AdvieskeuzeViewContext.CurrentProductgroep == null)
          return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        else
          return RedirectToRoute(AdvieskeuzeRoutes.Zoekresultaat(AdvieskeuzeViewContext.CurrentProductgroep));
      }
      if (model.Medewerker == null) {
        if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
          return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowLocatie(model.LocatieSlugStatus.Locatie));
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowLocatie(model.Locatie, AdvieskeuzeViewContext.CurrentProductgroep));
      }
      if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return RedirectToRoutePermanent(AdvieskeuzeRoutes.ShowAdviseur(model.LocatieSlugStatus.Locatie, model.Persoon));
      if (!model.Medewerker.IsZichtbaar)
        return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(model.Locatie, AdvieskeuzeViewContext.CurrentProductgroep));
      SitemapTitleFormatter.RegisterObjectForFormatting(new { adviseur = model.Persoon.VolledigeNaam, locatie = model.Locatie.Naam });
      AdvieskeuzeViewContext.CanonicalUrlRouteValuesSet(RouteUtils.AnonymousObjectToHtmlAttributes(AdvieskeuzeRoutes.ShowAdviseur(model.Locatie, model.Persoon, (Scope)null), null, null), HttpContext);
      if (model.Persoon.Foto != null)
        AdvieskeuzeViewContext.OpenGraphAzureImageSet($"{model.Persoon.Foto.ID}{model.Persoon.Foto.Extensie}");
      return View(model);
    }
  }
}
