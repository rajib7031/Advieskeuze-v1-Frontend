using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.ProductgroepAlgemeen {
  public class BeoordelingAlgemeenAreaRegistration : AreaRegistration {
    public override string AreaName => "BeoordelingAlgemeen";

    public override void RegisterArea(AreaRegistrationContext context) {
      var productgroepDetectieFactory = new DetectAreaTypeRouteFactory(new DataContext());
      context.MapRouteLowercase(
        "BeoordelingAlgemeen_index",
        "{beoordelingmoduleslug}/beoordeling/index/{id}",
        new { area = "BeoordelingAlgemeen", controller = "Beoordeling", action = "Index", id = UrlParameter.Optional },
        new { beoordelingmoduleslug = productgroepDetectieFactory.CreateDetectBeoordelingmoduleRoute(true), id = new GuidRouteConstraint(), },
        new string[] { "Advieskeuze.Areas.BeoordelingAlgemeen.Controllers" }
      );
      context.MapRouteLowercase(
        "BeoordelingAlgemeen_default",
        "{beoordelingmoduleslug}/beoordeling/{action}/{paginanr}",
        new { area = "BeoordelingAlgemeen", controller = "Beoordeling", action = "Pagina", paginanr = UrlParameter.Optional },
        new { beoordelingmoduleslug = productgroepDetectieFactory.CreateDetectBeoordelingmoduleRoute(true), },
        new string[] { "Advieskeuze.Areas.BeoordelingAlgemeen.Controllers" }
      );
    }
  }
}
