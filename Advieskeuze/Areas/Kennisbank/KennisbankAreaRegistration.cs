using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Kennisbank {
  public class KennisbankAreaRegistration : AreaRegistration {
    public override string AreaName => "Kennisbank";

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRouteLowercase(
          "Kennisbank_default",
          "Kennisbank/{controller}/{action}/{id}",
          new { controller = "Home", action = "Index", id = UrlParameter.Optional, area = "Kennisbank" },
          null,
          new string[] { "Advieskeuze.Areas.Kennisbank.Controllers" }
      );
    }
  }
}
