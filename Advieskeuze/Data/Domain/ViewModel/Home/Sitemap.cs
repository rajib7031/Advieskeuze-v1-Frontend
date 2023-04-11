using System.Collections.Generic;

namespace Advieskeuze.Data.Domain.ViewModel.Home {
  public class Sitemap {
    public IEnumerable<AdvieskeuzeCode.Data.IProductgroepCache> Productgroepen { get; set; }
    public List<string> Steden { get; set; }
  }
}
