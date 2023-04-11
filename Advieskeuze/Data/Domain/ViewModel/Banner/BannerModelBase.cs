using Advieskeuze.Areas.Advies.Controllers;

namespace Advieskeuze.Data.Domain.ViewModel.Banner {
  public class BannerModelBase : BannerController.IDefaultValidate {
    public AdvieskeuzeCode.Banners.Banner BannerSettings { get; set; }

    public bool RouteToBlank { get; set; }
    public string View { get; set; }
    public object PermaRedirect { get; set; }
  }
}