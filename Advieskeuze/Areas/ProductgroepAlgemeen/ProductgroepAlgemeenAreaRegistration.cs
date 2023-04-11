using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.ProductgroepAlgemeen {
  public class ProductgroepAlgemeenAreaRegistration : AreaRegistration {
    public override string AreaName => "ProductgroepAlgemeen";

    public override void RegisterArea(AreaRegistrationContext context) {
      var productgroepDetectieFactory = new DetectAreaTypeRouteFactory(new DataContext());
      context.MapRouteLowercase(
        "ProductgroepAlgemeen1_default",
        "{productgroeparea}/{controller}/{action}/{id}",
        new { area = "ProductgroepAlgemeen", controller = "Home", action = "Index", id = UrlParameter.Optional },
        new { productgroeparea = productgroepDetectieFactory.CreateDetectProductgroepRoute(true), controller = @"^(zoekresultaat|home)$" },
        new string[] { "Advieskeuze.Areas.ProductgroepAlgemeen.Controllers" }
      );
      context.MapRouteLowercase(
        "ProductgroepAlgemeen2_default",
        "{productgroeparea}/{action}",
        new { area = "ProductgroepAlgemeen", controller = "Home", action = "Zoek" },
        new { productgroeparea = productgroepDetectieFactory.CreateDetectProductgroepRoute(true), controller = @"^(home)$" },
        new string[] { "Advieskeuze.Areas.ProductgroepAlgemeen.Controllers" }
      );
    }
  }
}
