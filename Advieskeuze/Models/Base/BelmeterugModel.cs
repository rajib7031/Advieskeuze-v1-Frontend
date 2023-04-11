using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using SharedCode;
using System.ComponentModel.DataAnnotations;

namespace Advieskeuze.Models.Base {
  public class BelmeterugModel {
    public BelmeterugModel() {
    }
    public BelmeterugModel(Locatie locatie, IProductgroepCache productgroep) {
      Locatie = locatie;
      Invulformulier = new BelmeterugMakenModel();
    }
    public Locatie Locatie { get; set; }
    public BelmeterugMakenModel Invulformulier { get; set; }
  }

  public class BelmeterugMakenModel {
    [Display(Name = "Je naam")]
    [StringLength(50, ErrorMessage = "Mag niet langer dan 50 tekens zijn")]
    public string Naam { get; set; }

    [Display(Name = "Je telefoonnummer")]
    [Required(ErrorMessage = "Verplicht")]
    [RegularExpression(Formatter.TelefoonRegExp, ErrorMessage = "Geen geldig telefoonnummer")]
    public string Telefoon { get; set; }
  }
}
