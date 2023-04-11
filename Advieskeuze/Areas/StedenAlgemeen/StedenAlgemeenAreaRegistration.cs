using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.StedenAlgemeen {
  public class StedenAlgemeenAreaRegistration : AreaRegistration {
    public override string AreaName => "StedenAlgemeen";

    public override void RegisterArea(AreaRegistrationContext context) {
      var stedenDetectieFactory = new DetectAreaTypeRouteFactory(new DataContext());
      context.MapRouteLowercase(
        "StedenAlgemeen_default",
        "{stadslug}/{controller}/{action}/{id}",
        new { area = "StedenAlgemeen", controller = "Home", action = "Index", id = UrlParameter.Optional },
        new { stadslug = stedenDetectieFactory.CreateDetectStedenRoute(), controller = @"^(home)$", },
        new string[] { "Advieskeuze.Areas.StedenAlgemeen.Controllers" }
      );
    }
  }
}
