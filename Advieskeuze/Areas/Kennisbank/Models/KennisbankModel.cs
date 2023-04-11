using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Areas.Kennisbank.Models {
  public class KennisbankModel {

    public IEnumerable<Artikel> Artikelen { get; set; }
    public IEnumerable<Categorie> Categorieen { get; set; }
    public int AlleArtikelenCount { get; set; }

    public static KennisbankModel Create() {
      var model = new KennisbankModel {
        Categorieen = KennisbankData.GetCategorieen(),
        Artikelen = KennisbankData.GetArtikelenProminent(),
        AlleArtikelenCount = KennisbankData.GetArtikelen_Cache().Count
      };
      return model;
    }
  }
}