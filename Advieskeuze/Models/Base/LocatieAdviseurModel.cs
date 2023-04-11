using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Models.Base {
  public class LocatieAdviseurModel {
    public Locatie Locatie { get; set; }
    public LocatieSlugHelper LocatieSlugStatus { get; set; }
    public LocatieAdviseur Medewerker { get; set; }
    public Adviseur Persoon { get; set; }
    public IEnumerable<LocatieAdviseur> WerkzaamBijLocaties { get; set; }
    public IEnumerable<LocatieAdviseur> AndereMedewerkers { get; set; }
    public IEnumerable<Scope> Specialisaties { get; set; }
    public IEnumerable<AdviseurKwaliteitcontrole> AdviseurRegisters { get; set; }

    public static LocatieAdviseurModel Create(string postcode, string slugLocatie, string slugAdviseur) {
      var model = new LocatieAdviseurModel();
      model.LocatieSlugStatus = LocatieData.GetLocatieSlugResolve(postcode, slugLocatie);
      if (new[] { LocatieSlugHelper.LocatieSlugStatus.onbekend, LocatieSlugHelper.LocatieSlugStatus.goedMaarOnzichtbaar }.Contains(model.LocatieSlugStatus.Status))
        return model;
      model.Locatie = model.LocatieSlugStatus.Locatie as Locatie;
      model.Persoon = AdviseurData.GetAdviseur(model.Locatie.ID, slugAdviseur);
      if (model.Persoon != null) {
        model.Medewerker = model.Persoon.LocatieAdviseur.FirstOrDefault(la => la.LocatieID == model.Locatie.ID);
        model.AdviseurRegisters = KwaliteitcontroleData.GetAdviseurKwaliteitcontroleLijst(model.Locatie.ID).Where(ak => ak.IsGoedgekeurd && ak.AdviseurID == model.Persoon.ID).GroupBy(k => new { k.Kwaliteitcontrole.KorteNaam, k.Registratiecode }).Select(k => k.FirstOrDefault());
      }
      if (model.LocatieSlugStatus.Status == LocatieSlugHelper.LocatieSlugStatus.oud)
        return model;
      if (model.Medewerker != null) {
        model.WerkzaamBijLocaties = model.Persoon.LocatieAdviseur;
        model.AndereMedewerkers = model.WerkzaamBijLocaties.Count() == 1 ? AdviseurData.GetAdviseurs(model.Locatie, true).ToList() : new List<LocatieAdviseur>();
        model.Specialisaties = model.Medewerker.Persoon.Scope;
      }
      return model;
    }
  }
}