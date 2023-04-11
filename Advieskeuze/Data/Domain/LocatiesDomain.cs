using Advieskeuze.Data.Domain.ViewModel.Locaties;
using AdvieskeuzeCode;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.BI.Advieskeuze.LocatieModel;
using AdvieskeuzeCode.BI.Common.LocatieModel;
using AdvieskeuzeCode.BI.Common.MedewerkerModel;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Searchengine;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using LocatieData = AdvieskeuzeCode.BI.Advieskeuze.LocatieData;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// Returns information for locations in a DTO class only used by the location-detail page.
  /// All the rest is directly returned out of the <see cref="LocatieData"/> class.
  /// </summary>
  public sealed class LocatiesDomain {
    private readonly DataContextFactory _context;
    private readonly EnvironmentDomain.EnvironmentToken _environment;
    private readonly RequestToken _request;
    private LocatieData LocatieBI => new LocatieData(_context);
    private FotoData FotoBI => new FotoData(_context);
    private OpeningstijdData OpeningstijdBI => new OpeningstijdData(_context);
    private MedewerkerData MedewerkerBI => new MedewerkerData(_context);

    public LocatiesDomain(DataContextFactory context, RequestToken request, EnvironmentDomain.EnvironmentToken environment) {
      if (context == null)
        throw new ArgumentNullException("context");
      if (environment == null)
        throw new ArgumentNullException("environment ");
      _context = context;
      _environment = environment;
      _request = request;
    }

    #region Temp ExternID
    /// <summary>
    /// ExternID was used in the legacy framework of Advieskeuze. It can be removed if all references to it are gone.
    /// Some sadly are set in Banners that are active on websites of advisors, so it'll be required to do this conversion for some time.
    /// </summary>
    public KantoorLink GetVoorLinkByInternOrExternId(Guid id) {
      return LocatieBI.GetVoorLinkByInternOrExternId(id);
    }
    #endregion

    public async Task<KantoorDetailModel> KantoorDetails(string postcode, string slug) {
      var model = new KantoorDetailModel();
      model.Productgroep = _environment.Productgroep;
      var kantoorLinkStatus = LocatieBI.GetKantoorSlug(_request, postcode, slug);
      model.KantoorLink = kantoorLinkStatus.Locatie;
      if (kantoorLinkStatus.Status != LocatieSlugHelper.LocatieSlugStatus.goed) {
        model.Probleem = kantoorLinkStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud ? KantoorDetailModel.OphalenFout.KantoorLinkOud : KantoorDetailModel.OphalenFout.KantoorOnbekend;
        return model;
      }
      var entity = kantoorLinkStatus.Locatie as IKantoorEntity;
      var locatieID = entity.ID;
      model.Kantoor = await LocatieBI.DirecteProperties(entity);
      model.Openingstijden = await OpeningstijdBI.KantoorOpeningstijden(entity);
      model.LocatieRegisters = AdvieskeuzeCode.Data.KwaliteitcontroleData.GetLocatie(locatieID).Where(lk => lk.IsGoedgekeurd).GroupBy(k => new { k.Kwaliteitcontrole.KorteNaam, k.Registratiecode }).Select(k => k.FirstOrDefault()).Include(k => k.Kwaliteitcontrole).OrderByDescending(k => k.Kwaliteitcontrole.KorteNaam);
      model.Fotos = FotoBI.KantoorFotos(entity);
      model.Score = LocatieBI.KantoorScore(entity, model.Kantoor);
      model.RecenteReviews = AdvieskeuzeCode.Data.BeoordelingData.GetZichtbaarFrontend(model.Kantoor).OrderByDescending(b => b.ZichtbaarVanaf).Take(5);
      model.ReviewPortals = ReviewPortalLocatieData.Get(locatieID);

      // Tarieven
      var locatiescopes = LocatieScopeData.Get(entity.ID).Include("Scope");
      model.Tarieven = locatiescopes.Select(s => new TariefBasis() {
        LinkPostcode = model.KantoorLink.KantoorPostcode,
        LinkSlug = model.KantoorLink.KantoorSlug,
        IsZakelijk = s.Scope.IsZakelijk,
        Omschrijving = s.Scope.Naam,
        Slug = s.Scope.Slug,
        Tarief = s.Scope.ZoekresultaatTariefSoort.HasValue && s.Scope.ZoekresultaatTariefSoort.Value > 0 ?
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Advies ? s.Adviestarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Afsluiten ? s.Afsluitentarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Combinatie ? s.Combinatietarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Maatwerk ? s.Maatwerktarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Service ? s.Servicetarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Uur ? s.Uurtarief :
          (TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value == TariefSoort.Verkort ? s.Verkorttarief :
          null :
          null,
        TypeOmschrijving = s.Scope.ZoekresultaatTariefSoort.HasValue && s.Scope.ZoekresultaatTariefSoort.Value > 0 ? ((TariefSoort)s.Scope.ZoekresultaatTariefSoort.Value).ToString() : ""
      }).ToList();

      model.LocatieKwaliteitcontroles = await LocatieBI.LocatieKwaliteitcontroles(entity);
      model.ViaLink = new KantoorDetailItemHelper.Link();

      // The location can have a active tarief based on the scope the page is viewed from, set it here:
      if (model.Productgroep != null) {
        model.ActiefTarief = model.Tarieven.FirstOrDefault(t => t.Slug == model.Productgroep.Slug);
        model.Tarieven.Remove(model.ActiefTarief);
        model.ViaLink.VanuitZakelijk = model.Productgroep.IsZakelijk;
        model.ViaLink.VanuitParticulier = !model.Productgroep.IsZakelijk;
      }

      model.FocusZakelijk = model.ViaLink.VanuitZakelijk || (!model.ViaLink.VanuitParticulier && model.Kantoor.FocusZakelijk.HasValue && model.Kantoor.FocusZakelijk.Value);

      var contactOpties = LocatieBI.GetContactOpties(locatieID);
      model.ContactOpties = new List<KantoorContactOpties>();
      foreach (int optie in Enum.GetValues(typeof(SharedCode.ContactOptiesType))) {
        var contactOptieNew = new KantoorContactOpties() { ContactOptieOrder = 10 + optie, ContactOptieType = (SharedCode.ContactOptiesType)optie }; // Contactopties die niet ingevuld zijn onderaan het lijstje tonen op volgorde van de Enum
        var contactOptie = contactOpties.FirstOrDefault(c => c.ContactType == optie);
        if (contactOptie != null) {
          contactOptieNew.ContactOptieActief = contactOptie.IsActief;
          contactOptieNew.ContactOptieOrder = contactOptie.SortOrder;
          contactOptieNew.ContactOptieTekst = contactOptie.LinkTekst;
          contactOptieNew.ContactOptieType = (SharedCode.ContactOptiesType)contactOptie.ContactType;
        }
        model.ContactOpties.Add(contactOptieNew);
      }
      model.ContactOpties = model.ContactOpties.OrderBy(c => c.ContactOptieOrder).ToList();

      model.MedewerkersRaw = await MedewerkerBI.KantoorMedewerkers(entity, model.Productgroep != null ? model.Productgroep.ID : (Guid?)null);
      model.Medewerkers = model.MedewerkersRaw.Where(m => m.IsPromintent).ToList();
      if (!model.Medewerkers.Any()) {
        model.Medewerkers = model.MedewerkersRaw.ToList();
        model.MedewerkersOverige = new List<KantoorMedewerker>();
      }
      else
        model.MedewerkersOverige = model.MedewerkersRaw.Where(m => !model.Medewerkers.Contains(m)).ToList();
      // Only show 15 advisors in the first overview
      if (model.Medewerkers.Count() > 15) {
        var id15 = model.Medewerkers.Take(15).Select(m => m.ID);
        var idMore = model.Medewerkers.Where(m => !id15.Contains(m.ID)).Select(m => m.ID);
        model.MedewerkersOverige = model.MedewerkersOverige.Concat(model.Medewerkers.Where(m => idMore.Contains(m.ID))); // Do a concat because someone might have more than 15 IsProminent advisors.
        model.Medewerkers = model.Medewerkers.Where(m => id15.Contains(m.ID));
      }

      model.Blogs = BlogData.GetBlogs(model.Kantoor.ID).Where(b => b.IsGepubliceerd && b.IsGoedgekeurd).Take(10);
      var rawPartners = LocatiePartnerData.GetPartnersVoorLocatie(model.Kantoor.ID);
      model.Partners = rawPartners.Select(p => p.Locatie).Union(rawPartners.Select(p => p.Partner))
        .Where(l => l.ID != model.Kantoor.ID && l.StatusCode <= (int)LocatieStatus.Actief)
        .Distinct(new AdvieskeuzeCode.Data.Utils.DistinctEntityWithID<Locatie>())
        .ToList();
      return model;
    }
  }
}
