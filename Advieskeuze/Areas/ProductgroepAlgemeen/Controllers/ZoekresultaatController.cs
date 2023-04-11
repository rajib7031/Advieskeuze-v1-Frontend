using Advieskeuze.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using AdvieskeuzeCode.Searchengine;
using AdvieskeuzeCode.Searchengine.Models;
using SharedCode;
using System.Web.Mvc;

namespace Advieskeuze.Areas.ProductgroepAlgemeen.Controllers {
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Zoekresultaat)]
  public class ZoekresultaatController : Controller {
    [BannedIPCheck]
    public ActionResult Index(string geolocation, int sort = (int)SortType.LaatsteReview, string f = "") {
      var scope = AdvieskeuzeViewContext.CurrentProductgroep;
      if (scope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      SearchModel model = new SearchModel(scope.Slug, CampagneActiviteitContext.Current.QueryStringParameter, f);
      if (model.Scope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      model.GeoLocation = geolocation;
      model.Sort = sort;
      model.Scope.VoegProductgroepAanSitemapToe(new SitemapTitleFormatter());
      return View(model);
    }

    public ActionResult Filteren(string geolocation, int sort = (int)SortType.LaatsteReview, string f = "") {
      var scope = AdvieskeuzeViewContext.CurrentProductgroep;
      if (scope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      SearchModel model = new SearchModel(scope.Slug, CampagneActiviteitContext.Current.QueryStringParameter, f);
      if (model.Scope == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      model.GeoLocation = geolocation;
      model.Sort = sort;
      return View(model);
    }

    public RedirectToRouteResult In(string id, int? sortering = null, string locatie = null) {
      var pg = AdvieskeuzeViewContext.CurrentProductgroep;
      if (pg == null)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      if (!string.IsNullOrEmpty(locatie))
        id = locatie;
      return RedirectToRoute(AdvieskeuzeRoutes.ZoekenIn(pg, id, sortering));
    }
  }
}
