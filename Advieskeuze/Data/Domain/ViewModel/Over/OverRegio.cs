using AdvieskeuzeCode.BI.Advieskeuze.OverModel;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Data.Domain.ViewModel.Over {
  public class OverRegio {
    private RegioType? _redirectType;
    public RegioType? RedirectType {
      get { return _redirectType.HasValue ? _redirectType : (Data != null ? Data.RedirectType : null); }
      set { _redirectType = value; }
    }

    public IOverRegio Data { get; set; }
    public AdvieskeuzeCode.Data.IProductgroepCache Productgroep { get; set; }

    public abstract class OverRegioBase {
      public RegioType? RedirectType { get; set; }
      public string ProvincieNaam { get; set; }
      public string ProvincieSlug { get; set; }
      public string GemeenteNaam { get; set; }
      public string GemeenteSlug { get; set; }
      public string PlaatsNaam { get; set; }
      public string PlaatsSlug { get; set; }
      public int AantalBeoordelingen { get; set; }
      public IEnumerable<AdvieskeuzeCode.DataModel.Beoordeling> RecenteKantoorBeoordelingen { get; set; }
    }
    public class Landelijk : OverRegioBase, IOverRegio {
      public RegioType Type => RegioType.Landelijk;
      public string RegioOmschrijving => "Nederland";
      public string RegioNaam => "Nederland";
      public IEnumerable<OverRegioHelper.INaamSlugCombinatie> Provincies { get; set; }
    }
    public class Provincie : OverRegioBase, IOverRegio {
      public RegioType Type => RegioType.Provincie;
      public string RegioNaam => ProvincieNaam;
      public string RegioOmschrijving => "Nederland " + ProvincieNaam;
      public IEnumerable<OverRegioHelper.INaamSlugCombinatie> Gemeenten { get; set; }
    }
    public class Gemeente : OverRegioBase, IOverRegio {
      public RegioType Type => RegioType.Gemeente;
      public string RegioNaam => GemeenteNaam;
      public string RegioOmschrijving => "Nederland " + ProvincieNaam;
      public IEnumerable<AdvieskeuzeCode.DataModel.Plaats> Plaatsen { get; set; }
    }
    public class Plaats : OverRegioBase, IOverRegio {
      public RegioType Type => RegioType.Plaats;
      public string RegioNaam => PlaatsNaam;
      public string RegioOmschrijving => PlaatsNaam + " " + ProvincieNaam;
      public IEnumerable<Locatie> AlleKantoren { get; set; }

      public string GemiddeldeScoreDisplay { get; set; }
      public string GemiddeldeScoreAantalBeoordelingen { get; set; }
    }

    public enum RegioType {  // nummers bewust oplopend voor volgorde hack
      Onbekend = 0,
      Landelijk = 1,
      Provincie = 2,
      Gemeente = 3,
      Plaats = 4,
    }

    public interface IOverRegio {
      RegioType Type { get; }
      RegioType? RedirectType { get; }
      string RegioOmschrijving { get; }
      string RegioNaam { get; }
      string ProvincieNaam { get; }
      string ProvincieSlug { get; }
      string GemeenteNaam { get; }
      string GemeenteSlug { get; }
      string PlaatsNaam { get; }
      int AantalBeoordelingen { get; }
      IEnumerable<AdvieskeuzeCode.DataModel.Beoordeling> RecenteKantoorBeoordelingen { get; }
    }
  }
}
