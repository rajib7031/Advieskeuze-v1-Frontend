using AdvieskeuzeCode.BI.Advieskeuze.CompanyModel;
using AdvieskeuzeCode.DataModel;

namespace Advieskeuze.Data.Domain.ViewModel.Banner {
  public class BannerModelCompany : BannerModelBase {
    public Company Company { get; set; }
    public ScoreOverzicht Score { get; set; }
  }
}
