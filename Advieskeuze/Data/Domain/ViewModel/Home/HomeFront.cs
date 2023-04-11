using AdvieskeuzeCode.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Advieskeuze.Data.Domain.ViewModel.Home {
  public class HomeFront {
    public IEnumerable<SelectListItem> ParticuliereProductgroepenLijst { get; set; }
    public IEnumerable<SelectListItem> ZakelijkeProductgroepenLijst { get; set; }
    public IEnumerable<IProductgroepCache> ParticuliereProductgroepen { get; set; }
    public IEnumerable<IProductgroepCache> ZakelijkeProductgroepen { get; set; }
    public int AantalLocaties { get; set; }
    public int AantalAdviseurs { get; set; }
    public int AantalBeoordelingen { get; set; }
    public int AantalZoekopdrachten { get; set; }
    public int AantalPartnersites { get; set; }
    public IEnumerable<AdvieskeuzeCode.DataModel.Beoordeling> Beoordelingen { get; set; }
    public IEnumerable<AdvieskeuzeCode.DataModel.PartnerWebsite> Partnersites { get; set; }
    public IProductgroepCache Productgroep { get; set; }

    [Required(ErrorMessage = "Kies een onderwerp")]
    public string HomeProductgroep { get; set; }
    public string PlaatsPrefill { get; set; }


    public class Redirect {
      public string Zoektekst { get; set; }
      public IProductgroepCache Productgroep { get; set; }
    }
  }
}
