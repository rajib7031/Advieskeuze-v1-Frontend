using AdvieskeuzeCode;
using AdvieskeuzeCode.BI.Common.BeoordelingModel;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.LocatiePropertyFactory;
using AdvieskeuzeCode.Data.Score;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Models.Base {
  public class LocatieModel {
    public Locatie Locatie { get; set; }
    public LocatieSlugHelper LocatieSlugStatus { get; set; }
    public IQueryable<Beoordeling> LocatieBeoordelingen { get; set; }
    public IPagination<Beoordeling> BeoordelingenPagination { get; set; }
    public Locatie.CoordinaatHelper Coordinaat { get; set; }
    public IPropertyBase Properties { get; set; }
    public string C { get; set; }
    public IBeoordelingLocatie Beoordeling { get; set; }
    public Beoordeling Review { get; set; }
    public string ExterneReviewSite { get; set; }
    public int BeoordelingCount { get; set; }
    public LocatieScoreDetails ScoreDetails { get; set; }
    public string ScopeFilter { get; set; }
    public int? ScoreFilter { get; set; }
    public List<int> ScoreFilterList {
      get {
        if (ScoreFilter.HasValue) {
          List<int> scores = new List<int>();
          foreach (var c in ScoreFilter.Value.ToString()) {
            scores.Add(Convert.ToInt32(c.ToString()));
          }
          return scores;
        }
        return null;
      }
    }
    public int Reviews1Ster { get; set; }
    public int Reviews2Sterren { get; set; }
    public int Reviews3Sterren { get; set; }
    public int Reviews4Sterren { get; set; }
    public int Reviews5Sterren { get; set; }

    public static LocatieModel CreateDetailsSimpel(RequestToken request, string postcode, string slug) {
      var model = new LocatieModel();
      model.LocatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, slug);
      if (model.LocatieSlugStatus.Status != LocatieSlugHelper.LocatieSlugStatus.goed)
        return model;
      model.Locatie = model.LocatieSlugStatus.Locatie as Locatie;
      return model;
    }
    /// <param name="scoreFilter">Wordt gebruikt om reviews met specifieke scores te tonen (bijv 24 = reviews met score 2 of 4).</param>
    public static LocatieModel CreateBeoordelingLijst(RequestToken request, string postcode, string slug, int? scoreFilter, string scopeFilter) {
      var model = new LocatieModel();
      model.ScopeFilter = scopeFilter;
      model.ScoreFilter = scoreFilter;
      if (!model.ScoreFilter.HasValue)
        model.ScoreFilter = 12345;
      model.LocatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, slug);
      if (model.LocatieSlugStatus.Status != LocatieSlugHelper.LocatieSlugStatus.goed)
        return model;
      model.Locatie = model.LocatieSlugStatus.Locatie as Locatie;
      var reviews = BeoordelingData.GetFrontend(model.Locatie);
      model.ScoreDetails = new LocatieScoreDetails(reviews);
      model.BeoordelingCount = reviews.Count();
      if (!string.IsNullOrEmpty(model.ScopeFilter))
        reviews = reviews.Where(r => r.Scope.Slug == model.ScopeFilter);
      model.Reviews1Ster = reviews.Count(r => r.Waardering == 1);
      model.Reviews2Sterren = reviews.Count(r => r.Waardering == 2);
      model.Reviews3Sterren = reviews.Count(r => r.Waardering == 3);
      model.Reviews4Sterren = reviews.Count(r => r.Waardering == 4);
      model.Reviews5Sterren = reviews.Count(r => r.Waardering == 5);
      reviews = reviews.Where(r => model.ScoreFilterList.Contains(r.Waardering.Value));
      model.Coordinaat = model.Locatie.Coordinaat;
      model.Properties = model.Locatie.Property;
      model.LocatieBeoordelingen = reviews.OrderByDescending(b => b.ZichtbaarVanaf);
      model.C = CampagneActiviteitContext.Current.QueryStringParameter;
      return model;
    }
    public static LocatieModel CreateBeoordeling(RequestToken request, string postcode, string slug, string slugbeoordeling) {
      var model = new LocatieModel();
      var beoordeling = BeoordelingData.GetFrontend(slugbeoordeling);  // locatie die we via beoordeling slug krijgen is leidend

      model.LocatieSlugStatus = LocatieData.GetLocatieComplexSlugResolve(request, postcode, slug);
      if (beoordeling == null)
        model.Beoordeling = null;

      var locatie = model.LocatieSlugStatus.Locatie as Locatie;
      if (locatie == null)
        return model;
      if (beoordeling != null) {
        model.Beoordeling = beoordeling;
        model.Review = BeoordelingData.GetFrontendBeoordeling(slugbeoordeling);
      }

      model.Locatie = locatie;
      model.Coordinaat = model.Locatie.Coordinaat;
      model.Properties = model.Locatie.Property;
      model.LocatieBeoordelingen = BeoordelingData.GetFrontend(model.Locatie);  // Deze beoordelingen zijn al gefilterd op locatie type
      model.BeoordelingCount = model.LocatieBeoordelingen.Count();
      model.ScoreDetails = new LocatieScoreDetails(model.LocatieBeoordelingen);
      return model;
    }
  }
}