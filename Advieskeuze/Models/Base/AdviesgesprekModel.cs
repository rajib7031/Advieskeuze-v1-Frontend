using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using SharedCode;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Models.Base {
  public class AdviesgesprekModel {
    public AdviesgesprekModel() {
    }
    public AdviesgesprekModel(Locatie locatie, IProductgroepCache productgroep, string medewerker) {
      Locatie = locatie;
      JaNee = SelectListFactory.CreateJaNeeLijst(false);
      Invulformulier = new AdviesgesprekMakenModel();
      Invulformulier.ToestemmingVervolgTraject = false;
      Invulformulier.ContactMomentSlug = AdviesgesprekContactMomentType.Snel.ToString();
      ProductgroepList = SelectListFactory.CreateProductgroepMeervoudLijst(locatie.Scopes.Select(p => p.ID));
      if (!string.IsNullOrEmpty(medewerker)) {
        var adviseur = AdviseurData.GetAdviseur(medewerker, Locatie.ID);
        if (adviseur != null && adviseur.IsZichtbaar)
          Adviseur = adviseur.Persoon;
      }
      if (!ProductgroepList.Any())
        SelectListFactory.CreateProductgroepMeervoudLijst(ProductgroepData.GetProductgroepen_Cached().Select(p => p.ID));
      if (productgroep != null)
        Invulformulier.ProductgroepSlug = productgroep.Slug;
      ContactMomentList = SelectListFactory.CreateContactMomentLijst();
    }

    public Locatie Locatie { get; set; }
    public Adviseur Adviseur { get; set; }
    public IEnumerable<SelectListItem> JaNee { get; set; }
    public AdviesgesprekMakenModel Invulformulier { get; set; }
    public IEnumerable<SelectListItem> ProductgroepList { get; set; }
    public IEnumerable<SelectListItem> ContactMomentList { get; set; }
  }

  public class AdviesgesprekMakenModel {
    [Display(Name = "Je telefoonnummer")]
    //[Required(ErrorMessage = "Verplicht")]
    [RegularExpression(Formatter.TelefoonRegExp, ErrorMessage = "Geen geldig telefoonnummer")]
    public string Telefoon { get; set; }

    [Display(Name = "Je e-mailadres")]
    [Required(ErrorMessage = "Verplicht")]
    [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres")]
    public string Emailadres { get; set; }

    [Display(Name = "Je naam")]
    [Required(ErrorMessage = "Verplicht")]
    [StringLength(50, ErrorMessage = "Mag niet langer dan 50 tekens zijn")]
    public string Naam { get; set; }

    [Display(Name = "Je bedrijfsnaam (optioneel)")]
    [StringLength(100, ErrorMessage = "Mag niet langer dan 100 tekens zijn")]
    public string Bedrijfsnaam { get; set; }

    [Display(Name = "Mogen we je over een paar weken vragen een review te geven over dit kantoor?")]
    [Required(ErrorMessage = "Verplicht")]
    public bool ToestemmingVervolgTraject { get; set; }

    [Display(Name = "Over welk onderwerp wil je geadviseerd worden?")]
    [Required(ErrorMessage = "Verplicht")]
    public string ProductgroepSlug { get; set; }

    [Display(Name = "Op welk moment wil je dat het kantoor contact met je opneemt?")]
    [Required(ErrorMessage = "Verplicht")]
    public string ContactMomentSlug { get; set; }
  }
}
