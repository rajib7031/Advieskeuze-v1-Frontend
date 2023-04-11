using Advieskeuze.Data;
using Advieskeuze.Data.Domain;
using AdvieskeuzeCode.Banners;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Advieskeuze.Areas.Advies.Controllers {
  /// <summary>
  /// Banner implementatie
  /// </summary>
  /// Deze controller voorziet in de uiteindelijke banners. Dat kunnen kantoor, kantoor-adviseur of
  /// keten banners zijn.
  /// 
  /// Het idee is dat men een klein stukje script op hun website plaatst. Dit stukje script roept
  /// een javascript bestand aan op het CDN. 
  /// De code op het CDN is specifiek voor het kantoor, de banner en de gevraagde instellingen. Hij
  /// valideert deze ook en schakelt een voorbeeldmodus is als er dingen niet in orde zijn.
  /// De output van het CDN is een javascript replace die een iFrame renderd. Deze iFrame verwijst
  /// naar de GO controller voor de banners.
  /// De GO controller van de banners verwijst uiteindelijk naar deze controller.
  /// 
  /// Er is een vastgestelde selectie aan banners en instellingen per banner. Dit is vastgelegd in
  /// de BannerFactory. Wijzigingen moeten backwards compatible zijn, vooral wat afmeting betreft,
  /// zodat gebruikers niet opeens aangepaste websites hebben.
  /// 
  /// Er is logging in het CDN wie welke banners met welke instellingen gebruikt.
  /// 
  /// Bij het renderen van de banner zal deze controller zo veel mogelijk proberen te laten zien
  /// ook al is de vraag niet in orde. Zo blijft de layout van de website van de klant op orde.
  /// Wel zal hij een voorbeeldmodus activeren als je geen recht op de banner of een foutmelding
  /// tonen als er meer niet in orde is.
  /// 
  /// Het extranet heeft een 'bouwer' waarmee je de banner kan samenstellen. Deze probeert je
  /// banners zo te tonen zoals ze er uit zien, ook al is de informatie niet op orde. Daarom doet
  /// het extranet meteen al een aanvraag voor voorbeeldmodus zodat ook een ontbrekende score toch
  /// ff laat zien hoe het er uit zou zien.
  /// 
  /// De properties zijn een efficiente samenvoeging van instellingen.
  /// (Eerst werd er met key-values gewerkt maar dat bleek bewerkelijk.)
  /// 
  /// Deze banners zouden niet direct aangeroepen moeten zijn. Dus je zou in theorie makkelijk de
  /// linkjes aan kunnen passen. De praktijk is weerbarstiger en je moet dus oppassen met het aanpassen
  /// van de aanroep van banners.
  /// <remarks>
  /// De banners hebben de naam 'widgets' gehad en deze kom je nog her-en-der tegen.
  /// </remarks>
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Locatie)]
  [SessionState(SessionStateBehavior.Disabled)]
  public class BannerController : BaseController {
    new BannerDomain DomainContext => base.DomainContext.GetBanner();

    public async Task<ActionResult> BannerLinkReview(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "linkreview", legacyMode));
    }

    public async Task<ActionResult> BannerBeoordeelOnsKantoor(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "beoordeelonskantoor", legacyMode));
    }

    public async Task<ActionResult> BannerBeoordeelMijAdviseur(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "beoordeelmijadviseur", legacyMode));
    }

    public async Task<ActionResult> BannerBeoordeelMijTekst(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "beoordeelmijtekst", legacyMode));
    }

    public async Task<ActionResult> BannerBeoordeelMijPlaatje(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "beoordeelmijplaatje", legacyMode));
    }

    public async Task<ActionResult> BannerLinkKantoor(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "linkkantoor", legacyMode));
    }

    public async Task<ActionResult> BannerLinkKantoorTekst(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "linkkantoortekst", legacyMode));
    }

    public async Task<ActionResult> BannerScore(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "score", legacyMode));
    }

    public async Task<ActionResult> BannerScoreUitgebreid(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "scoreuitgebreid", legacyMode));
    }

    public async Task<ActionResult> BannerScoreUitgebreidEnReviews(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "scoreuitgebreidenreviews", legacyMode));
    }

    public async Task<ActionResult> BannerScoreUitgebreidEnReviewsSmal(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "scoreuitgebreidenreviewssmal", legacyMode));
    }

    public async Task<ActionResult> BannerScoreEnReviewsPagina(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "scoreenreviewspagina", legacyMode));
    }

    public async Task<ActionResult> BannerAmbassadeur(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "ambassadeur", legacyMode));
    }

    // Legacy
    public async Task<ActionResult> WidgetScore(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "score", legacyMode));
    }

    public async Task<ActionResult> WidgetAmbassadeur(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "ambassadeur", legacyMode));
    }

    public async Task<ActionResult> WidgetKlantwaardering(string postcode, string slug, string properties, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, Request.Url != null ? ParameterString(HttpUtility.ParseQueryString(Request.Url.Query)) : null, "klantwaardering", legacyMode));
    }

    public async Task<ActionResult> WidgetBeoordelingen(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "beoordelingen", legacyMode));
    }

    public async Task<ActionResult> WidgetBeoordelingenGroot(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "beoordelingengroot", legacyMode));
    }

    public async Task<ActionResult> WidgetBeoordelingenKlein(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "beoordelingenklein", legacyMode));
    }

    public async Task<ActionResult> WidgetBeoordeelMijKantoor(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "beoordeelmijkantoor", legacyMode));
    }

    public async Task<ActionResult> WidgetBeoordeelMijAdviseur(string postcode, string slug, string ws, bool legacyMode = true) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, ws, "beoordeelmijadviseur", legacyMode));
    }

    /// <summary>
    /// Maatwerk voor de Hypotheekbond
    /// MAG WEG!
    /// </summary>
    public async Task<ActionResult> BannerReviewsLaatsteTwee(string postcode, string slug, string properties, bool legacyMode = false) {
      return DefaultAction(await DomainContext.LocatieBanner(postcode, slug, properties, "reviewslaatstetwee", legacyMode));
    }

    public ActionResult BannerKetenScore(string slug, string properties, bool legacyMode = false) {
      return DefaultAction(DomainContext.CompanyBanner(slug, properties, "ketenscore", legacyMode));
    }

    public ActionResult BannerKetenLinkReview(string slug, string properties, bool legacyMode = false) {
      return DefaultAction(DomainContext.CompanyBanner(slug, properties, "ketenlinkreview", legacyMode));
    }

    internal ActionResult DefaultAction<T>(T model) where T : IDefaultValidate {
      if (model == null || model.RouteToBlank)
        return RedirectToRoute(AdvieskeuzeRoutes.Blank());
      if (model.PermaRedirect != null)
        return RedirectToRoutePermanent(model.PermaRedirect);
      return View(model.View, model);
    }
    public interface IDefaultValidate {
      bool RouteToBlank { get; }
      string View { get; }
      object PermaRedirect { get; }
    }

    private static string ParameterString(NameValueCollection parseString) {
      var result = new StringBuilder();
      if (!parseString.HasKeys())
        return result.ToString();
      foreach (string key in parseString)
        result.Append($"{BannerFactory.GetLegacyParameterName(key) ?? key}={parseString[key]}&");
      result.Length--;
      return result.ToString();
    }
  }
}
