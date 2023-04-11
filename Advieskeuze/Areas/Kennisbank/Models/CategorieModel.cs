using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Areas.Kennisbank.Models {
  public class CategorieModel {

    public Categorie Categorie { get; set; }
    public IEnumerable<Artikel> Etalage { get; set; }
    public int AlleArtikelenCount { get; set; }

    public static CategorieModel Create(string categorieSlug) {
      var model = new CategorieModel {
        Categorie = KennisbankData.GetCategorie(categorieSlug),
        Etalage = KennisbankData.GetEtalageArtikelen(categorieSlug),
        AlleArtikelenCount = KennisbankData.GetArtikelen_Cache().Count
      };
      return model;
    }
  }
}