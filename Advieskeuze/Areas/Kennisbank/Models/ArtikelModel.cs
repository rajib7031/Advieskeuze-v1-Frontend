using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.Zoeken;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Areas.Kennisbank.Models {
  public class ArtikelModel {
    public Artikel Artikel { get; set; }
    public List<Artikel> GerelateerdeArtikelen { get; set; }
    public string Sleutelwoord { get; set; }
    public string ZoekTerm { get; set; }

    public static ArtikelModel Create(string slug, string sleutelwoord) {
      var model = new ArtikelModel { Sleutelwoord = sleutelwoord, Artikel = KennisbankData.GetArtikel(slug) };
      if (model.Artikel != null && model.Artikel.Sleutelwoord.Any()) {
        model.GerelateerdeArtikelen = new List<Artikel>();
        foreach (var woord in model.Artikel.Sleutelwoord) {
          model.GerelateerdeArtikelen.AddRange(KennisbankZoeker.ZoekGerelateerdeArtikelen(woord.Sleutelwoord1));
        }
        model.GerelateerdeArtikelen = model.GerelateerdeArtikelen.Distinct().Where(a => a.IsActief).ToList();
        model.GerelateerdeArtikelen.Remove(model.Artikel);
      }
      return model;
    }
  }
}
