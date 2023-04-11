using AdvieskeuzeCode.BI.Common.LocatieModel;
using AdvieskeuzeCode.BI.Common.MedewerkerModel;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.Data;
using System.Collections.Generic;

namespace Advieskeuze.Data.Domain.ViewModel.Banner {
  public class BannerModelLocatie : BannerModelBase {
    public IKantoorSlugLink KantoorLink { get; set; }
    public IKantoor Kantoor { get; set; }
    public IProductgroepLink ProductgroepLink { get; set; }
    public KantoorMedewerker Medewerker { get; set; }
    public KantoorDetail KantoorUitgebreid { get; set; }
    public KantoorScoreEnBeoordeling Score { get; set; }
    public IEnumerable<AdvieskeuzeCode.DataModel.Beoordeling> Reviews { get; set; }
  }
}