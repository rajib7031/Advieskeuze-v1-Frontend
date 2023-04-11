using AdvieskeuzeCode.Banners;
using SharedCode;
using SharedCode.Mvc;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Advies {
  public class AdviesAreaRegistration : AreaRegistration {
    public override string AreaName => "Advies";

    public override void RegisterArea(AreaRegistrationContext context) {
      context.MapRouteLowercase(
        "Advies_Locatie",
        "{areaadvies}/{postcode}/{slug}/{action}/{slug2}",
        new { area = "Advies", controller = "Locatie", action = "Details", postcode = UrlParameter.Optional, slug = UrlParameter.Optional, slug2 = UrlParameter.Optional },
        new { postcode = Formatter.PostcodeRegExp, action = @"^(details|beoordelingen|beoordeling|bekijkbeoordeling|adviesgesprek|adviesgesprekbedankt|betrouwbaarheidsstempel|belmeterug|belmeterugbedankt|tarieven)$" },
        new[] { "Advieskeuze.Areas.Advies.Controllers" }
      );
      context.MapRouteLowercase(
        "Advies_LocatieBanners",
        "{areaadvies}/{postcode}/{slug}/{action}",
        new { area = "Advies", controller = "Banner", postcode = UrlParameter.Optional, slug = UrlParameter.Optional },
        new { postcode = Formatter.PostcodeRegExp, action = @"^(" + BannerFactory.LocatieBannerRouteRestrictrie + ")$" },
        new[] { "Advieskeuze.Areas.Advies.Controllers" }
      );
      context.MapRouteLowercase(
        "Advies_Adviseur",
        "{areaadvies}/{postcode}/{sluglocatie}/{slugadviseur}/{action}",
        new { area = "Advies", controller = "Adviseur", action = "Details", postcode = UrlParameter.Optional, sluglocatie = UrlParameter.Optional, slugadviseur = UrlParameter.Optional },
        new { postcode = Formatter.PostcodeRegExp, action = @"^(details)$" },
        new[] { "Advieskeuze.Areas.Advies.Controllers" }
      );
      context.MapRouteLowercase(
        "Advies_LocatieVergelijk",
        "{areaadvies}/Vergelijk/{id}",
        new { area = "Advies", controller = "Locatie", action = "Vergelijk", },
        null,
        new[] { "Advieskeuze.Areas.Advies.Controllers" }
      );
      context.MapRouteLowercase(
        "Advies_default",
        "Advies/{controller}/{action}/{id}",
        new { areaadvies = "Advies", area = "Advies", controller = "Home", action = "Index", id = UrlParameter.Optional },
        new { },
        new[] { "Advieskeuze.Areas.Advies.Controllers" }
      );
    }
  }
}