using AdvieskeuzeCode;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.Zoeken;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using SharedCode.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Models {
  public class HomeModel {
    public IPagination<Beoordeling> Beoordelingen { get; set; }
    public int AantalLocaties { get; set; }
    public LocatieFilter Filter { get; set; }
    public IEnumerable<SelectListItem> ProductgroepenLijst { get; set; }
    public IEnumerable<SelectListItem> ProductgroepenLijstZakelijk { get; set; }
    public IEnumerable<IProductgroepCache> Productgroepen { get; set; }
    public IEnumerable<IProductgroepCache> ProductgroepenZakelijk { get; set; }

    [MaxLength(50, ErrorMessage = "de plaats kan maximaal 50 tekens lang zijn.")]
    public string IngevuldePlaats { get; set; }

    [MaxLength(50, ErrorMessage = "de naam van het bedrijf kan maximaal 50 tekens lang zijn.")]
    public string IngevuldeBedrijfsnaam { get; set; }
    public Guid? IngevuldeLocatieId { get; set; }
    public Guid? IngevuldeCompanyId { get; set; }

    [MaxLength(50, ErrorMessage = "de naam van de dienstverlener kan maximaal 50 tekens lang zijn.")]
    public string IngevuldeDienstverlener { get; set; }
    public Guid? IngevuldeDienstverlenerId { get; set; }

    [Required(ErrorMessage = "maak een keuze")]
    public string IngevuldeProductgroep { get; set; }

    public bool ValideLocatie { get; set; }
    public Locatie Locatie { get; set; }
    public LocatieSlugHelper LocatieSlugStatus { get; set; }
    public IProductgroepCache Productgroep { get; set; }
    public string AdviseurSlug { get; set; }
    public bool IsDemo { get; set; }

    public string BeoordelaarEmail { get; set; }
    public string BeoordelaarNaam { get; set; }
    public int? Waardering { get; set; }

    public HomeModel() {
      Filter = new LocatieFilter();
      ProductgroepenLijst = new List<SelectListItem>();
    }

    public static HomeModel CreateBeoordelingen(RequestToken request, string postcode, string kantoorSlug, string adviseurSlug, bool? isDemo, string klantemail, string klantnaam, int? waardering = null) {
      var model = new HomeModel();
      model.ValideLocatie = !string.IsNullOrEmpty(postcode) && !string.IsNullOrEmpty(kantoorSlug);
      if (model.ValideLocatie) {
        model.LocatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, kantoorSlug);
        if (model.LocatieSlugStatus.Status != LocatieSlugHelper.LocatieSlugStatus.goed)
          return model;
        model.Locatie = model.LocatieSlugStatus.Locatie as Locatie;
        model.AdviseurSlug = adviseurSlug;
        model.IsDemo = isDemo.HasValue && isDemo.Value;
        model.BeoordelaarNaam = klantnaam;
        model.BeoordelaarEmail = klantemail;
        model.Waardering = waardering;
      }
      var beoordelingen = BeoordelingData.GetZichtbaarFrontend();

      if (model.Locatie != null) {
        model.Productgroepen = model.Locatie.Scopes.Where(p => !p.IsZakelijk).ToList();
        model.ProductgroepenZakelijk = model.Locatie.Scopes.Where(p => p.IsZakelijk).ToList();
        if (model.Locatie.FocusZakelijk.HasValue && model.Locatie.FocusZakelijk.Value) {
          if (model.ProductgroepenZakelijk.Any()) {
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Zakelijk--", Value = "" } });
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(SelectListFactory.CreateProductgroepLijstZakelijk(model.Locatie, "AdviseurNaam"));
          }
          if (model.Productgroepen.Any()) {
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Particulier--", Value = "" } });
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(SelectListFactory.CreateProductgroepLijstParticulier(model.Locatie, "AdviseurNaam"));
          }
        }
        else {
          if (model.Productgroepen.Any()) {
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Particulier--", Value = "" } });
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(SelectListFactory.CreateProductgroepLijstParticulier(model.Locatie, "AdviseurNaam"));
          }
          if (model.ProductgroepenZakelijk.Any()) {
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Zakelijk--", Value = "" } });
            model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(SelectListFactory.CreateProductgroepLijstZakelijk(model.Locatie, "AdviseurNaam"));
          }
        }

        beoordelingen = beoordelingen.Where(b => b.LocatieID == model.Locatie.ID);
      }
      else {
        model.Productgroepen = ProductgroepData.GetProductgroepenParticulier_Cached().ToList();
        model.ProductgroepenZakelijk = ProductgroepData.GetProductgroepenZakelijk_Cached().ToList();
        model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Particulier--", Value = "" } });
        model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new SelectList(model.Productgroepen.Where(p => p.DefaultBeoordelingmoduleID.HasValue), "slug", "AdviseurNaam"));
        model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new[] { new SelectListItem() { Text = "--Zakelijk--", Value = "" } });
        model.ProductgroepenLijst = model.ProductgroepenLijst.Concat(new SelectList(model.ProductgroepenZakelijk.Where(p => p.DefaultBeoordelingmoduleID.HasValue).Select(p => new { slug = p.Slug, AdviseurNaam = p.AdviseurNaam + " (" + p.Naam.ToLower() + ")" }), "slug", "AdviseurNaam"));
      }
      model.Beoordelingen = beoordelingen.Where(b => b.StatusCode == (int)BeoordelingStatus.Goedgekeurd && b.Toelichting.Length >= 140 && b.Naam != "anoniem").OrderByDescending(b => b.ZichtbaarVanaf).Take(5).AsPagination(1, 3);
      return model;
    }
  }
}
