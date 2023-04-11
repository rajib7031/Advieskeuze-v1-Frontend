using Advieskeuze.Data.Domain.ViewModel.Banner;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Banners;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.Routes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// Domain voor banner controller in advies area
  /// </summary>
  public sealed class BannerDomain {
    private readonly DataContextFactory _context;
    private readonly EnvironmentDomain.EnvironmentToken _environment;
    private readonly RequestToken _request;
    private LocatieData LocatieBI => new LocatieData(_context);
    private MedewerkerData MedewerkerBI => new MedewerkerData(_context);
    private CacheData CacheBI => new CacheData(_context);

    public BannerDomain(DataContextFactory context, RequestToken request, EnvironmentDomain.EnvironmentToken environment) {
      if (context == null)
        throw new ArgumentNullException("context");
      if (environment == null)
        throw new ArgumentNullException("environment");
      _context = context;
      _environment = environment;
      _request = request;
    }

    public async Task<BannerModelBase> LocatieBanner(string postcode, string slug, string properties, string bannerslug, bool legacyMode) {
      var slugStatus = LocatieBI.GetKantoorSlug(_request, postcode, slug);
      if (slugStatus.Status == AdvieskeuzeCode.Data.LocatieSlugHelper.LocatieSlugStatus.oud)
        return new BannerModelBase() { PermaRedirect = AdvieskeuzeRoutes.BannerBijNaam(_request.PaginaAction, properties, slugStatus.Locatie, legacyMode) };
      if (slugStatus.Status == AdvieskeuzeCode.Data.LocatieSlugHelper.LocatieSlugStatus.onbekend)
        return new BannerModelBase() { RouteToBlank = true };
      var model = new BannerModelLocatie();
      model.KantoorLink = slugStatus.Locatie;
      model.Kantoor = LocatieBI.DirecteProperties(slugStatus.Entity.ID);
      var toegang = model.Kantoor.HeeftMarketingpakket;
      model.BannerSettings = BannerFactory.GetBannerApplySettings(model.Kantoor, bannerslug, properties, toegang);
      model.BannerSettings.LegacyMode = legacyMode;
      model.View = model.BannerSettings.Slug;
      if (model.BannerSettings.ProductgroepRelevant && !string.IsNullOrEmpty(model.BannerSettings.PropertyContainer.Productgroep))
        model.ProductgroepLink = CacheBI.GetProductgroep(model.BannerSettings.PropertyContainer.Productgroep);
      if (model.BannerSettings.AdviseurRelevant)
        model.Medewerker = MedewerkerBI.KantoorMedewerker(model.Kantoor, model.BannerSettings.PropertyContainer.Adviseur);
      var gebruiktScore = model.View.Contains("review") || model.View.Contains("score");
      if (model.BannerSettings.LogoIPVFotoRelevant || gebruiktScore)
        model.KantoorUitgebreid = await LocatieBI.DirecteProperties(model.Kantoor);
      if (gebruiktScore) {
        model.Score = LocatieBI.KantoorScore(model.Kantoor, model.KantoorUitgebreid, aantalReviews: model.BannerSettings.PropertyContainer.AantalReviews, metOordeelVragen: model.View.Contains("pagina"));
        model.Reviews = AdvieskeuzeCode.Data.BeoordelingData.GetZichtbaarFrontend(model.Kantoor, AdvieskeuzeCode.DataModel.BeoordelingStatus.Goedgekeurd).OrderByDescending(b => b.ZichtbaarVanaf).Take(model.BannerSettings.PropertyContainer.AantalReviews);
      }
      return model;
    }

    public BannerModelBase CompanyBanner(string slug, string properties, string bannerslug, bool legacyMode) {
      var model = new BannerModelCompany();
      model.Company = AdvieskeuzeCode.Data.CompanyData.Get(slug);
      model.RouteToBlank = model.Company == null;
      if (model.RouteToBlank)
        return model;
      var access = model.Company.HasBehaviorType(AdvieskeuzeCode.DataModel.BehaviorType.Banners);
      model.BannerSettings = BannerFactory.GetBannerApplySettings(model.Company, bannerslug, properties, access);
      model.BannerSettings.LegacyMode = legacyMode;
      model.View = bannerslug;
      model.Score = new AdvieskeuzeCode.BI.Advieskeuze.CompanyModel.ScoreOverzicht() { Waarde = AdvieskeuzeCode.Data.CompanyData.GetScore(model.Company.Id) };
      return model;
    }
  }
}