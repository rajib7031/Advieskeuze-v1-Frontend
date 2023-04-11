using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Data.Zoeken;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.DataModel.Sorting;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Advieskeuze.Areas.Expert.Models {
  public class ZoekresultaatModel {
    public CampagneActiviteit Activiteit { get; set; }
    public IEnumerable<ExpertVraag> Zoekresultaat { get; set; }
    public IPagination<ExpertVraag> ZoekresultaatPaged { get; set; }
    public IEnumerable<SleutelwoordHelper> Sleutelwoorden { get; set; }
    public ExpertplatformZoekerModel Resultaat { get; set; }
    public string ZoekTerm { get; set; }
    public Onderwerp Onderwerp { get; set; }

    public ZoekresultaatModel() {
      Activiteit = CampagneActiviteitContext.Current.TryGetActieveCampagneActiviteitOrFallbackAdvieskeuze();
    }

    public static ZoekresultaatModel Create(string onderwerpSlug, string zoekTerm, ZoekresultaatSorting sorting) {
      var model = new ZoekresultaatModel();
      var controlledZoekTerm = (zoekTerm + "").Trim();
      var onderwerpen = OnderwerpData.GetVoorVraag(model.Activiteit);
      if (onderwerpen != null)
        model.Onderwerp = onderwerpen.FirstOrDefault(o => o.Slug == onderwerpSlug);
      if (model.Onderwerp == null)
        model.Onderwerp = OnderwerpData.GetOnderwerp("financieel"); // We gebruiken alleen nog maar financieel als onderwerp. Dus even eenvoudig hardcoden.
      model.Resultaat = ExpertplatformZoeker.ZoekExpertVragen(controlledZoekTerm, model.Activiteit, model.Onderwerp);
      model.Zoekresultaat = MaybeSortResults(sorting, model.Resultaat.FinalZoekresultaten);
      model.Sleutelwoorden = model.Resultaat.RelevanteZoekwoorden.Where(s => s.Term != controlledZoekTerm).Take(10).ToList();
      return model;
    }

    private static IEnumerable<ExpertVraag> MaybeSortResults(ZoekresultaatSorting sorting, IQueryable<ExpertVraag> vragen) {
      // Sort on default or slider.
      if (string.IsNullOrEmpty(sorting.Column) || !GenericPropertyOrderer.CanSortOnProperty(sorting.Column, new ExpertVraag())) {
        return vragen
          .OrderByDescending(ev => ev.VraagStatusDatum);
      }
      // Sort on column
      if (sorting.Direction == SortDirection.Ascending)
        return vragen.ToList().OrderBy(l => GenericPropertyOrderer.OrderByProperty(sorting.Column, l)).ThenBy(ev => ev.VraagStatusDatum);
      return vragen.ToList().OrderByDescending(l => GenericPropertyOrderer.OrderByProperty(sorting.Column, l)).ThenByDescending(ev => ev.VraagStatusDatum);
    }
  }
}