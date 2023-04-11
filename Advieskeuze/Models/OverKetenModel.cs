using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Models {
  public class OverKetenModel {
    public Company Company { get; set; }
    public CampagneActiviteit ZoekresultaatCampagne { get; set; }
    public IEnumerable<Locatie> Locaties { get; set; }
    public bool HeeftScore { get; set; }
    public decimal? KantorenScore { get; set; }
    public string KantorenScoreDisplay { get; set; }
    public int AantalTotaal { get; set; }
    public IEnumerable<Beoordeling> Beoordelingen { get; set; }
  }
  public class OverRegisterModel {
    public Kwaliteitcontrole Register { get; set; }
    public int AantalEntiteiten { get; set; }
    public IEnumerable<Scope> Zoekresultaten { get; set; }
  }
}