using Advieskeuze.Data.Domain.ViewModel.Over;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BeoordelingData = AdvieskeuzeCode.BI.Advieskeuze.BeoordelingData;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// This returns information for some SEO-pages of Advieskeuze.nl. There are quite a number of pages generated from
  /// cities, provinces etc. They all return some information about what advisors are active in that given region.
  /// </summary>
  public sealed class OverDomain {
    private readonly DataContextFactory _context;
    private CacheData CacheBI => new CacheData(_context);
    private BeoordelingData BeoordelingenBI => new BeoordelingData(_context);
    private OverData OverBI => new OverData(_context);

    public OverDomain(DataContextFactory context) {
      if (context == null)
        throw new ArgumentNullException("context");
      _context = context;
    }

    public async Task<OverRegio> GetRegio(string productgroepSlug, string provincieSlug, string gemeenteSlug, string plaatsSlug) {
      var model = new OverRegio();
      model.Productgroep = CacheBI.GetProductgroep(productgroepSlug);
      if (model.Productgroep == null || !model.Productgroep.DefaultZoekmoduleID.HasValue)
        model.RedirectType = OverRegio.RegioType.Onbekend;
      else if (string.IsNullOrEmpty(provincieSlug))
        model.Data = await GetRegio(model.Productgroep);
      else if (string.IsNullOrEmpty(gemeenteSlug))
        model.Data = await GetRegio(model.Productgroep, provincieSlug);
      else if (string.IsNullOrEmpty(plaatsSlug))
        model.Data = GetRegio(model.Productgroep, provincieSlug, gemeenteSlug);
      else
        model.Data = GetRegio(model.Productgroep, provincieSlug, gemeenteSlug, plaatsSlug);
      return model;
    }
    private async Task<OverRegio.Landelijk> GetRegio(IProductgroepCache productgroep) {
      var model = new OverRegio.Landelijk();
      model.Provincies = await OverBI.GetProvincies();
      model.AantalBeoordelingen = CacheBI.GetAantalBeoordelingen(productgroep);
      model.RecenteKantoorBeoordelingen = BeoordelingenBI.RecentZichtbare(productgroep, 5);
      return model;
    }
    private async Task<OverRegio.Provincie> GetRegio(IProductgroepCache productgroep, string provincieSlug) {
      var model = new OverRegio.Provincie();
      model.ProvincieSlug = provincieSlug.ToLower();
      model.ProvincieNaam = provincieSlug.ToLower();
      model.Gemeenten = await OverBI.GetGemeenten(provincieSlug);
      if (!model.Gemeenten.Any()) {
        model.RedirectType = OverRegio.RegioType.Landelijk;
        return model;
      }
      var reviews = DataContext.Current.Beoordeling.Include("Locatie").Where(b => b.ProductgroepID == productgroep.ID && b.StatusCode == (int)BeoordelingStatus.Goedgekeurd && b.LocatieID.HasValue && b.Locatie.PlaatsID.HasValue && b.Locatie.Plaats1.Provincie.ToLower() == model.ProvincieSlug && b.ZichtbaarVanaf.HasValue).OrderByDescending(b => b.ZichtbaarVanaf.Value);
      model.AantalBeoordelingen = reviews.Count();
      model.RecenteKantoorBeoordelingen = reviews.Take(5);
      return model;
    }
    private OverRegio.Gemeente GetRegio(IProductgroepCache productgroep, string provincieSlug, string gemeenteSlug) {
      var model = new OverRegio.Gemeente();
      model.ProvincieSlug = provincieSlug.ToLower();
      model.ProvincieNaam = provincieSlug.ToLower();
      model.GemeenteSlug = gemeenteSlug.ToLower();

      var gemeente = DataContext.Current.Plaats.FirstOrDefault(p => p.GemeenteSlug.ToLower() == model.GemeenteSlug);
      model.GemeenteNaam = gemeente != null ? gemeente.Gemeente : null;
      if (string.IsNullOrEmpty(model.GemeenteNaam)) {
        model.RedirectType = OverRegio.RegioType.Provincie;
        return model;
      }

      var reviews = DataContext.Current.Beoordeling.Include("Locatie").Where(b => b.ProductgroepID == productgroep.ID && b.StatusCode == (int)BeoordelingStatus.Goedgekeurd && b.LocatieID.HasValue && b.Locatie.PlaatsID.HasValue && b.Locatie.Plaats1.GemeenteSlug.ToLower() == model.GemeenteSlug && b.ZichtbaarVanaf.HasValue).OrderByDescending(b => b.ZichtbaarVanaf.Value);

      model.Plaatsen = DataContext.Current.Plaats.Where(p => p.GemeenteSlug == model.GemeenteSlug);
      model.AantalBeoordelingen = reviews.Count();
      model.RecenteKantoorBeoordelingen = reviews.Take(5);
      return model;
    }
    private OverRegio.Plaats GetRegio(IProductgroepCache productgroep, string provincieSlug, string gemeenteSlug, string plaatsSlug) {
      var model = new OverRegio.Plaats();
      model.ProvincieSlug = provincieSlug.ToLower();
      model.ProvincieNaam = provincieSlug.ToLower();
      model.GemeenteSlug = gemeenteSlug.ToLower();
      model.PlaatsSlug = plaatsSlug.ToLower();

      var plaats = DataContext.Current.Plaats.FirstOrDefault(p => p.Naam.ToLower() == model.PlaatsSlug);

      model.GemeenteNaam = plaats != null ? plaats.Gemeente : null;
      if (string.IsNullOrEmpty(model.GemeenteNaam)) {
        model.RedirectType = OverRegio.RegioType.Provincie;
        return model;
      }
      model.PlaatsNaam = plaats != null ? plaats.Naam : null;
      if (string.IsNullOrEmpty(model.PlaatsNaam)) {
        model.RedirectType = OverRegio.RegioType.Gemeente;
        return model;
      }

      var locaties = DataContext.Current.Locatie.Where(b => b.StatusCode <= (int)LocatieStatus.Actief && b.Plaats.ToLower() == model.PlaatsSlug && b.LocatieScope.Any(ls => ls.ScopeID == productgroep.ID)).OrderBy(l => l.Naam);
      var locatieIds = locaties.Select(l => l.ID).ToList();
      var reviews = DataContext.Current.Beoordeling.Where(b => b.ProductgroepID == productgroep.ID && b.StatusCode == (int)BeoordelingStatus.Goedgekeurd && b.LocatieID.HasValue && locatieIds.Contains(b.LocatieID.Value) && b.ZichtbaarVanaf.HasValue).OrderByDescending(b => b.ZichtbaarVanaf.Value);

      model.AantalBeoordelingen = reviews.Count();
      model.AlleKantoren = locaties;
      model.RecenteKantoorBeoordelingen = reviews.Take(5);

      if (locaties != null && locaties.Any(l => l.Score.HasValue)) {
        var gemiddeldeScore = locaties.Where(l => l.Score.HasValue).Average(l => l.Score.Value);
        model.GemiddeldeScoreDisplay = Math.Round(gemiddeldeScore, 1) == 10 ? "10" : Math.Round(gemiddeldeScore, 1).ToString(CultureInfo.GetCultureInfo("en-US")); // Culture US voor de . separator (max score zonder separator)
        model.GemiddeldeScoreAantalBeoordelingen = model.AantalBeoordelingen.ToString();
      }
      return model;
    }
  }
}
