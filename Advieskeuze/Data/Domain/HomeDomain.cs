using Advieskeuze.Data.Domain.ViewModel.Home;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Common.EntityLinq;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BeoordelingData = AdvieskeuzeCode.BI.Advieskeuze.BeoordelingData;
using FaqData = AdvieskeuzeCode.BI.Advieskeuze.FaqData;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// This handles some data calls for the homepage and its surrounding pages (faq, partnersites etc).
  /// </summary>
  public sealed class HomeDomain {
    private readonly DataContextFactory _context;
    private readonly EnvironmentDomain.EnvironmentToken _environment;
    private IProductgroepCacheData ProductgroepBI => new ProductgroepCacheFactory(_context).GetGlobal();
    private CacheData CacheBI => new CacheData(_context);
    private FaqData FaqBI => new FaqData(_context);

    public HomeDomain(DataContextFactory context, EnvironmentDomain.EnvironmentToken environment) {
      if (context == null)
        throw new ArgumentNullException("context");
      if (environment == null)
        throw new ArgumentNullException("environment");
      _context = context;
      _environment = environment;
    }

    /// <summary>
    /// This model is used on the homepage and returns some cached counts and scope-lists.
    /// </summary>
    public HomeFront GetFront() {
      var model = new HomeFront();
      model.ParticuliereProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeParticuliereProductgroepenLijst(), "Key", "Text");
      model.ZakelijkeProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeZakelijkeProductgroepenLijst(), "Key", "Text");
      model.AantalBeoordelingen = CacheBI.GetAantalBeoordelingen();
      model.AantalZoekopdrachten = CacheBI.GetAantalZoekopdrachtenAfgelopenMaand();
      model.AantalLocaties = CacheBI.GetAantalLocaties();
      model.AantalAdviseurs = CacheBI.GetAantalAdviseurs();
      model.AantalPartnersites = CacheBI.GetAantalPartnersites();
      model.Beoordelingen = AdvieskeuzeCode.Data.BeoordelingData.GetZichtbaarFrontend().MetStatus(BeoordelingStatus.Goedgekeurd).Where(b => b.Toelichting.Length >= 140 && b.Naam != "anoniem").OrderByDescending(b => b.ZichtbaarVanaf).Take(2);
      model.Partnersites = PartnerWebsiteData.Get().Where(p => p.IsEtalage).Take(7);
      return model;
    }
    public HomeFront.Redirect GetFrontRedirect(string zoektekst, string productgroepSlug) {
      var model = new HomeFront.Redirect();
      model.Productgroep = ProductgroepBI.ProductgroepViaSlug(productgroepSlug);
      model.Zoektekst = zoektekst.Trim();
      return model;
    }

    public HomeFront GetParticulier() {
      var model = new HomeFront();
      model.ParticuliereProductgroepen = ProductgroepData.GetProductgroepenParticulier_Cached();
      model.ParticuliereProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeParticuliereProductgroepenLijst(), "Key", "Text");
      return model;
    }
    public HomeFront GetZakelijk() {
      var model = new HomeFront();
      model.ZakelijkeProductgroepen = ProductgroepData.GetProductgroepenZakelijk_Cached();
      model.ZakelijkeProductgroepenLijst = new SelectList(SelectListFactoryCached.CreateHomeZakelijkeProductgroepenLijst(), "Key", "Text");
      return model;
    }

    /// <summary>
    /// The list of partnerwebsite is extended with some hardcoded companies.
    /// This is done since not all of them are companies in our database, some are "landelijke speler" and some are sub-companies.
    /// </summary>
    public HomeFront GetPartnerWebsites() {
      var model = new HomeFront();
      model.Partnersites = new List<PartnerWebsite>();
      model.Partnersites = model.Partnersites.Concat(new List<PartnerWebsite>() {
        new PartnerWebsite() { Titel = "De Hypotheker", PortalUrl = "https://www.hypotheker.nl/beoordeel-de-hypotheker/", VoorbeeldAfbeeldingID = Guid.Parse("17ea232f-3492-4995-a28e-c0f11f13198f") },
        new PartnerWebsite() { Titel = "De Hypotheekshop", PortalUrl = "https://www.hypotheekshop.nl/", VoorbeeldAfbeeldingID = Guid.Parse("d48925ad-33c9-4ee0-9370-9d560b209f58") },
        new PartnerWebsite() { Titel = "Finzie", PortalUrl = "https://finzie.nl/", VoorbeeldAfbeeldingID = Guid.Parse("74b50034-ae79-4810-9eb2-1da35312459e") },
        new PartnerWebsite() { Titel = "Huis en Hypotheek", PortalUrl = "https://www.huis-hypotheek.nl/", VoorbeeldAfbeeldingID = Guid.Parse("c929bf84-c900-4bf1-b7d7-036d48a2c644") },
        new PartnerWebsite() { Titel = "Hypokeur", PortalUrl = "https://www.hypokeur.nl/", VoorbeeldAfbeeldingID = Guid.Parse("8919d295-7396-4453-9a6e-78dcd0dc3f83") },
        new PartnerWebsite() { Titel = "Hypotheek Visie", PortalUrl = "https://www.hypotheekvisie.nl/", VoorbeeldAfbeeldingID = Guid.Parse("cc24f867-0ddb-4144-9708-0c1315ab241d") },
        new PartnerWebsite() { Titel = "Rabobank", PortalUrl = "https://www.rabobank.nl/particulieren/", VoorbeeldAfbeeldingID = Guid.Parse("ac4c521e-2199-4370-9c66-eb16398981f0") },
        new PartnerWebsite() { Titel = "SNS Bank", PortalUrl = "https://www.snsbank.nl/particulier/home.html", VoorbeeldAfbeeldingID = Guid.Parse("95a445e8-1aa8-4872-9c42-94b327bef69f") },
        new PartnerWebsite() { Titel = "Van Bruggen Adviesgroep", PortalUrl = "https://www.vanbruggen.nl/", VoorbeeldAfbeeldingID = Guid.Parse("bccc7ea6-ef34-44ba-8aff-0ab8df9e0bbb") },
        new PartnerWebsite() { Titel = "VLIEG Financieel Advies", PortalUrl = "https://www.vlieg.nl/", VoorbeeldAfbeeldingID = Guid.Parse("4b62109d-d348-471e-8960-dbb66e473a2a") },
        new PartnerWebsite() { Titel = "FDC", PortalUrl = "https://www.fdc.nl/", VoorbeeldAfbeeldingID = Guid.Parse("9d584b5f-d9a8-44a1-b831-b1408de38e8a") },
        new PartnerWebsite() { Titel = "Lancyr", PortalUrl = "https://www.lancyr.nl/", VoorbeeldAfbeeldingID = Guid.Parse("55c5ba63-784f-43dd-9745-4bd761a344b1") },
        new PartnerWebsite() { Titel = "Univé", PortalUrl = "https://www.unive.nl/zuidnederland", VoorbeeldAfbeeldingID = Guid.Parse("0b012ad4-bfee-405b-9ef0-b0e4dd122d92") },
        new PartnerWebsite() { Titel = "Veldsink Advies", PortalUrl = "https://www.veldsink.nl/", VoorbeeldAfbeeldingID = Guid.Parse("74ea75ef-9492-415f-a1f2-d063eb01eb58") },
        new PartnerWebsite() { Titel = "Viisi", PortalUrl = "https://www.viisi.nl/", VoorbeeldAfbeeldingID = Guid.Parse("41030e06-b2e5-41eb-a9ee-e33c2c7a59af") },
        new PartnerWebsite() { Titel = "NBG", PortalUrl = "https://www.nbg.nl/", VoorbeeldAfbeeldingID = Guid.Parse("b9d9634f-0ce6-4f13-b77d-f546f2fceeba") },
        new PartnerWebsite() { Titel = "Poliservice", PortalUrl = "https://poliservice.nl", VoorbeeldAfbeeldingID = Guid.Parse("76057323-fc32-4fe7-9fc2-b4c59d5874d0") },
        new PartnerWebsite() { Titel = "Hypotheek.nl", PortalUrl = "https://www.hypotheek.nl/", VoorbeeldAfbeeldingID = Guid.Parse("a65cb5ab-0f7e-4230-9a73-74ca739812b8") },
        new PartnerWebsite() { Titel = "Hypotheekrente.nl", PortalUrl = "https://www.hypotheekrente.nl/", VoorbeeldAfbeeldingID = Guid.Parse("43e6ec25-a124-4a1e-8e6c-5bd0dbcc2ef7") },
        new PartnerWebsite() { Titel = "MoneyWise", PortalUrl = "https://www.moneywise.nl/", VoorbeeldAfbeeldingID = Guid.Parse("91f9a556-0d03-4696-8335-5eee34c446f6") },
        new PartnerWebsite() { Titel = "Hoekstra & Van Eck", PortalUrl = "https://hoekstraenvaneck.nl/", VoorbeeldAfbeeldingID = Guid.Parse("d48925ad-33c9-4ee0-9370-9d560b209f58") },
      });
      model.Partnersites = model.Partnersites.Concat(PartnerWebsiteData.Get().ToList()).OrderBy(p => Randomizer.Instance.Random.Next(100));
      return model;
    }
    public Sitemap GetSitemap() {
      var model = new Sitemap();
      model.Productgroepen = ProductgroepBI.Productgroepen().Where(p => p.HeeftHome).ToList();
      model.Steden = PlaatsData.Top20Steden();
      return model;
    }

    public AdvieskeuzeCode.BI.Advieskeuze.FaqModel.FaqIndex GetFaqIndex() {
      return FaqBI.GetFaqIndex();
    }
    public AdvieskeuzeCode.BI.Advieskeuze.FaqModel.FaqIndex.FaqVraagIndividueel GetFaqVraag(Guid? vraagID) {
      return FaqBI.GetFaqVraag(vraagID);
    }
  }
}
