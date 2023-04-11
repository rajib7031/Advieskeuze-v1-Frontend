using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert {
  public class ExpertAreaRegistration : AreaRegistration {
    public override string AreaName => "Expert";

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRouteLowercase(
        "ExperRedirect",
        "Expert/",
        new { action = "Index", controller = "Home", area = "Expert" },
        null,
        new string[] { "Advieskeuze.Areas.Expert.Controllers" }
        );
      context.MapRouteLowercase(
        "Expert_default",
        "Expert/{controller}/{action}/{id}",
        new { action = "Index", id = UrlParameter.Optional, area = "Expert" },
        null,
        new string[] { "Advieskeuze.Areas.Expert.Controllers" }
        );
    }
  }
}
