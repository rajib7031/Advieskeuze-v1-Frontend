using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Routes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Advieskeuze.Areas.Expert.Models {
  public class VraagModel {
    public VraagModel() {
      Pagina1 = new VraagModelPagina1();
      Pagina2 = new VraagModelPagina2();
    }

    public int PaginaNr { get; set; }
    public VraagModelPagina1 Pagina1 { get; set; }
    public VraagModelPagina2 Pagina2 { get; set; }

    public static VraagModel Create(int paginaNr) {
      var model = new VraagModel { PaginaNr = paginaNr };
      return model;
    }
  }

  public class VraagModelPagina1 {
    [Required(ErrorMessage = "Dit veld is verplicht")]
    [Display(Name = "Wat is uw naam?")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Uw naam moet tussen de 2 en 50 tekens bevatten.")]
    public string Naam { get; set; }

    [Required(ErrorMessage = "Dit veld is verplicht")]
    [Display(Name = "Wat is uw e-mailadres?")]
    [MaxLength(200, ErrorMessage = "Een e-mailadres mag maximaal 200 tekens lang zijn")]
    [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres")]
    public string Emailadres { get; set; }
  }

  public class VraagModelPagina2 {
    public VraagModelPagina2() {
      var activiteit = CampagneActiviteitContext.Current.TryGetActieveCampagneActiviteitOrFallbackAdvieskeuze();
      OnderwerpList = SelectListFactory.CreateOnderwerpLijst(activiteit);
    }

    public IEnumerable<SelectListItem> OnderwerpList { get; set; }

    [Required(ErrorMessage = "Dit veld is verplicht")]
    [Display(Name = "Over welk onderwerp wilt u een vraag stellen?")]
    public Guid? Onderwerp { get; set; }

    [Required(ErrorMessage = "Dit veld is verplicht")]
    [Display(Name = "Stel uw vraag", Prompt = "Stel uw vraag in minimaal 10 en maximaal 1000 karakters")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Minimaal 10 en maximaal 1000 karakters")]
    public string Vraag { get; set; }
  }
}