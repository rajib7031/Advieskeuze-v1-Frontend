using AdvieskeuzeCode.Data.Zoeken;
using SharedCode.Mvc;

namespace Advieskeuze.Areas.Kennisbank.Models {
  public class ZoekresultaatModel {
    public IPagination<ZoekresultaatArtikel> Zoekresultaat { get; set; }
    public string ZoekTerm { get; set; }

    public static ZoekresultaatModel Create(string zoekTerm, ZoekresultaatSorting sorting) {
      return new ZoekresultaatModel {
        Zoekresultaat = KennisbankZoeker.ZoekArtikel(zoekTerm, sorting),
        ZoekTerm = zoekTerm,
      };
    }
  }
}