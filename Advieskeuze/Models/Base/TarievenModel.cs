using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Models.Base {
  public class TarievenModel {
    public TarievenModel(Locatie locatie) {
      Locatie = locatie;
    }

    public Locatie Locatie { get; set; }
    public Scope Scope { get; set; }
    public LocatieScope LocatieScope { get; set; }

    public IEnumerable<Adviseur> Specialisten { get; set; }

    public bool HeeftTarieven {
      get {
        return LocatieScope.Verkorttarief.HasValue || LocatieScope.Maatwerktarief.HasValue || LocatieScope.Combinatietarief.HasValue || LocatieScope.Adviestarief.HasValue || LocatieScope.Afsluitentarief.HasValue || LocatieScope.Servicetarief.HasValue || LocatieScope.Uurtarief.HasValue || !string.IsNullOrEmpty(LocatieScope.ToelichtingOpTarief) || !string.IsNullOrEmpty(LocatieScope.ToelichtingUrl);
      }
    }
  }
}
