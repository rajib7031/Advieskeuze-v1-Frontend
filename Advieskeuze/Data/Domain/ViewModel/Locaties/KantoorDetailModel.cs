using AdvieskeuzeCode.BI.Advieskeuze.LocatieModel;
using AdvieskeuzeCode.BI.Common.BeoordelingModel;
using AdvieskeuzeCode.BI.Common.BlogModel;
using AdvieskeuzeCode.BI.Common.LocatieModel;
using AdvieskeuzeCode.BI.Common.MedewerkerModel;
using AdvieskeuzeCode.BI.Common.OpeningstijdModel;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Data.Domain.ViewModel.Locaties {
  public class KantoorDetailModel {
    public IKantoorSlugLink KantoorLink { get; set; }
    public IProductgroepCache Productgroep { get; set; }
    public OphalenFout? Probleem { get; set; }
    public KantoorDetail Kantoor { get; set; }
    public IEnumerable<KantoorOpeningstijd> Openingstijden { get; set; }
    public IEnumerable<LocatieKwaliteitcontrole> LocatieRegisters { get; set; }
    public IEnumerable<IFotoBestandLink> Fotos { get; set; }
    public BeoordelingRender<IBeoordelingLocatie> RecenteBeoordelingen { get; set; }
    public IEnumerable<AdvieskeuzeCode.DataModel.Beoordeling> RecenteReviews { get; set; }
    public KantoorDetailItemHelper.Link ViaLink { get; set; }
    public KantoorScoreEnBeoordeling Score { get; set; }
    internal IEnumerable<KantoorMedewerker> MedewerkersRaw { get; set; }
    public IEnumerable<KantoorMedewerker> Medewerkers { get; set; }
    public IEnumerable<KantoorMedewerker> MedewerkersOverige { get; set; }
    public IEnumerable<Blog> Blogs { get; set; }
    public IEnumerable<Locatie> Partners { get; set; }
    public IEnumerable<ReviewPortalLocatie> ReviewPortals { get; set; }
    public List<KantoorContactOpties> ContactOpties { get; set; }
    public bool FocusZakelijk { get; set; }
    public IEnumerable<LocatieKwaliteitcontrole> LocatieKwaliteitcontroles { get; set; }
    public TariefBasis ActiefTarief { get; set; }
    public List<TariefBasis> Tarieven { get; set; }

    public IEnumerable<TariefBasis> TarievenZakelijk {
      get {
        return Tarieven.Where(t => t.IsZakelijk);
      }
    }
    public IEnumerable<TariefBasis> TarievenParticulier {
      get {
        return Tarieven.Where(t => !t.IsZakelijk);
      }
    }

    public enum OphalenFout {
      KantoorOnbekend,
      KantoorLinkOud,
      KantoorProductgroepOnbekend,
    }
  }
}
