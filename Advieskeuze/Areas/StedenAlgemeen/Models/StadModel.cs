using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Areas.StedenAlgemeen.Models {
  public class StadModel {
    public Plaats Plaats { get; set; }
    public int AantalDienstverleners { get; set; }
    public IEnumerable<Locatie> Dienstverleners { get; set; }
    public int AantalReviews { get; set; }
    public decimal GemiddeldeScore { get; set; }

    public static StadModel Create(string stadSlug) {
      var model = new StadModel();
      model.Plaats = PlaatsData.GetPlaats(stadSlug);
      model.Dienstverleners = LocatieData.GetZoekresultaatLocaties(model.Plaats.ID);
      model.AantalDienstverleners = model.Dienstverleners.Count();
      model.GemiddeldeScore = model.Dienstverleners.Average(l => l.Score).Value;
      model.AantalReviews = model.Dienstverleners.Sum(l => l.Beoordeling.Count(b => b.StatusCode == (int)BeoordelingStatus.Goedgekeurd));
      return model;
    }
  }
}