using Advieskeuze.Data;
using Advieskeuze.Data.Domain;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using System;
using System.Web.Mvc;

namespace Advieskeuze.Areas.BeoordelingAlgemeen.Controllers {
  /// <summary>
  /// Dit is de beoordeling-afneem render engine.
  /// </summary>
  /// Deze controller regelt de start en invoer van de beoordelingen.
  /// De index pakt je op en zet de extra parameters vast zoals vooringevuld kantoor of het beoordelen van een
  /// eerder gevoerd adviesgesprek. Dit komt in een 'sessie' te zitten.
  /// 
  /// De pagina's draaien in de VraagAntwoord engine. Dit is een generieke koppeling van antwoorden
  /// aan een entiteit. In dit geval aan beoordelingen.
  /// 
  /// Een beoordelingsmodule bevat de volgende pagina's:
  /// 1 - Vooraf
  /// 2 - Het gesprek
  /// 3 - Waardering
  /// (optioneel) 4 - Eigen vragen
  /// 5 - Verzenden
  /// 6 - Bevestigen (demogragische vragen)
  /// De engine zou toe kunnen staan om hier vanaf te wijken maar omdat dit nooit gebeurd is dit nu hardcoded
  /// vastgelegd als de pagina flow.
  /// 
  /// De eigen vragen worden dynamisch ingeladen op basis van de beoordelingsmodule en organisatie (via campagneactiviteit).
  /// 
  /// Stap 1 t/m 4 is ook beschikbaar in de web API, draaiend op dezelfde engine.
  /// 
  /// De view bouwt dynamisch op adhv de vragen die aan de pagina gekoppeld zijn.
  /// Er worden verschillende datatypen ondersteunt met verschillende visualisaties. Belangrijkste is wel de lijst
  /// koppeling waarmee uit de (in db beschikbare) selectlist gekozen kan worden.
  /// Een speciaal type selectie is de plaats / kantoor / adviseur koppeling waarmee je op kan zoeken wie je gesproken hebt.
  /// Ook zit er de mogelijkheid ingebouwd om vragen van elkaar af te laten hangen met tonen. Dit kan koppelen op dezelfde pagina
  /// of met een vraag op een voorgaande pagina.
  /// Verder zit er ook een puntsgewijze opsomming in dmv een 'kop' waaronder dan items vallen. In het geval van
  /// radiobuttons zal dit visueel een groep gaan vormen waardoor kolommen onstaan met headers.
  /// 
  /// De moderatietool heeft een uitgebreide editor waarmee beoordelingsroutes gemaakt kunnen worden.
  /// 
  /// Routing is zo gemaakt dat de productgroep slug ipv BeoordelingAlgemeen gebruikt wordt. Dit werkt samen met de
  /// productgroep landingspagina (en dus het zoekresultaat).
  /// <remarks>
  /// Een route kan ook uit staan. Daar kun je dan alsnog in komen door een sessie aan te maken
  /// die wel gekoppeld is aan die route.
  /// </remarks>
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Beoordeling)]
  public class BeoordelingController : BaseController {
    private string ModuleSlug => RouteData.Values["beoordelingmoduleslug"].ToString();
    private BeoordelingengineDomain _domainContext;
    public new BeoordelingengineDomain DomainContext { get { if (_domainContext == null) _domainContext = base.DomainContext.GetBeoordelingengine(ModuleSlug); return _domainContext; } }

    public RedirectToRouteResult Index(string postcode = null, string kantoor = null, string adviseur = null, Guid? id = null, Guid? sessieid = null, bool? isDemo = null, int? paginaNr = null, string klantemail = null, string klantnaam = null, Guid? companyid = null, int? waardering = null) {
      if (string.IsNullOrEmpty(postcode) && string.IsNullOrEmpty(kantoor) && string.IsNullOrEmpty(adviseur) && !id.HasValue && !sessieid.HasValue && !isDemo.HasValue && !companyid.HasValue)
        return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(ModuleSlug, null, paginaNr, Environment.CampagneActiviteit));
      var engine = DomainContext.StartIndex(postcode, kantoor, adviseur, id, isDemo, paginaNr, klantemail, klantnaam, companyid, waardering);
      return Redirect(engine);
    }

    public ActionResult Pagina(int? paginanr = null, Guid? sessieid = null) {
      var request = RequestUtil;
      var engine = DomainContext.Start(sessieid, gewenstePaginaNr: paginanr);
      if (!engine.KanEngineVerder)
        return Redirect(engine);
      var replaceData = DomainContext.GetReplaceVelden(engine);
      SiteMap.ObjectForFormatting = replaceData;
      var paginaData = DomainContext.RenderPagina(engine, engine.Module.OordeelOver, false, replaceData);
      DomainContext.UpdateWithLive(engine, paginaData.Data);
      ViewData["EngineProductgroep"] = engine.Productgroep;
      if (engine.SessieToken.CompanyId.HasValue)
        ViewData["CompanyId"] = engine.SessieToken.CompanyId.Value;
      AdvieskeuzeCode.Data.BezoekData.Add(request, CampagneActiviteitContext.Current.CampagneActiviteitTraceID, CampagneActiviteitContext.Current.CurrentScope); // TMOD LOG nieuwe mechanisme
      return View(paginaData);
    }
    [HttpPost]
    [HoneypotCaptcha("FirstName")]
    public ActionResult Pagina(FormCollection form, int? paginanr = null, Guid? sessieid = null, bool isDemo = false) {
      var engine = DomainContext.Start(sessieid, gewenstePaginaNr: paginanr, isSubmit: true, isDemo: isDemo);
      engine.KorteBeoordeling = engine.Module != null ? engine.Module.Slug.Contains("-kort") : false;
      if (!engine.KanEngineVerder)
        return Redirect(engine);
      var replaceData = DomainContext.GetReplaceVelden(engine);
      SiteMap.ObjectForFormatting = replaceData;
      var bindOnPagina = DomainContext.RenderPagina(engine, engine.Module.OordeelOver, true, replaceData);
      if (TryUpdateModel(bindOnPagina.Data, "Data")) {
        var livePagina = DomainContext.RenderPagina(engine, engine.Module.OordeelOver, true, replaceData);
        DomainContext.UpdateWithLive(engine, livePagina.Data);
        DomainContext.PatchLiveWithBinded(engine, livePagina.Data, bindOnPagina.Data);
        if (TryValidateModel(livePagina.Data, "Data")) {
          DomainContext.SaveChanges(engine, livePagina.Data);
          DomainContext.VolgendePagina(engine, livePagina.Data);
          return Redirect(engine);
        }
        ViewData["EngineProductgroep"] = engine.Productgroep;
        return View(livePagina);
      }
      return Redirect(engine);
    }

    private RedirectToRouteResult Redirect(IBeoordelingEngineToken engine) {
      if (!engine.KanEngineVerder) {
        if (engine.VerkeerdeStatus)
          return RedirectToRoute(AdvieskeuzeRoutes.BeoordelingBestaatFout(engine.BeoordelingID));
        if (engine.Productgroep != null)
          return RedirectToRoute(AdvieskeuzeRoutes.ProductgroepHome(engine.Productgroep));
        return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling());
      }
      if (engine.RouteAfgerond) {
        return RedirectToRoute(AdvieskeuzeRoutes.BeoordelingBevestigen(engine.BeoordelingID));
      }
      if (engine.RenderPaginaNr != engine.VolgendePaginaNr)
        return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(ModuleSlug, engine.SessieSecurityID, engine.VolgendePaginaNr, Environment.CampagneActiviteit));
      return RedirectToRoute(AdvieskeuzeRoutes.Beoordeling(ModuleSlug, engine.SessieSecurityID, engine.RenderPaginaNr, Environment.CampagneActiviteit));
    }
  }
}
