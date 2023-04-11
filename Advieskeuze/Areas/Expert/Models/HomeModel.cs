using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advieskeuze.Areas.Expert.Models {
  public class HomeModel {
    public HomeModel() {
      Activiteit = CampagneActiviteitContext.Current.TryGetActieveCampagneActiviteitOrFallbackAdvieskeuze();
    }
    public IEnumerable<Adviseur> Experts { get; set; }
    public IEnumerable<Onderwerp> Onderwerpen { get; set; }
    public IEnumerable<ExpertVraag> Vragen { get; set; }
    public IEnumerable<LocatieAdviseur> LocatieAdviseurs { get; set; }
    public ExpertVraag Vraag { get; set; }
    public IEnumerable<ExpertAntwoord> Antwoorden { get; set; }
    public IPagination<ExpertAntwoord> AntwoordenPagination { get; set; }
    public CampagneActiviteit Activiteit { get; set; }
    public Onderwerp Onderwerp { get; set; }
    public Adviseur Expert { get; set; }
    public ExpertAntwoord Antwoord { get; set; }
    public int AantalVragen { get; set; }

    public static HomeModel Create() {
      var model = new HomeModel();
      model.Onderwerpen = OnderwerpData.GetVoorVraag(model.Activiteit).Take(5);
      var vragen = ExpertVraagData.GetOverzichtZichtbare(model.Activiteit);
      model.Vragen = vragen.OrderByDescending(v => v.AanmaakDatum).Take(8);
      model.AantalVragen = vragen.Count();
      model.Experts = ExpertData.GetVoorOverzicht(model.Activiteit).Take(5);
      return model;
    }

    public static HomeModel CreateVraagDetails(string id) {
      var model = new HomeModel();
      model.Vraag = ExpertVraagData.GetComplex(model.Activiteit, id);  // Als je via complex en activiteit ophaalt dan zit er meteen een statuscheck en security check achter
      if (model.Vraag != null)
        model.Antwoorden = model.Vraag.ExpertAntwoord.Where(ea => ea.TeltAntwoord).ToList();
      return model;
    }

    public static HomeModel CreateOnderwerp(string onderwerpSlug) {
      var model = new HomeModel();
      model.Onderwerp = OnderwerpData.GetVoorVraag(model.Activiteit).FirstOrDefault(o => o.Slug == onderwerpSlug);
      if (model.Onderwerp == null)
        model.Onderwerp = CampagneActiviteitContext.Current.TryGetActieveCampagneActiviteitOrFallbackAdvieskeuze().Onderwerp;
      model.Vragen = ExpertVraagData.GetOverzichtZichtbare(model.Activiteit, model.Onderwerp).OrderByDescending(ev => ev.VraagStatusDatum).Take(50).ToList();
      model.Onderwerpen = OnderwerpData.GetVoorVraag(model.Activiteit).Take(5).ToList();
      return model;
    }

    public static HomeModel CreateAntwoordenBekijken(Guid? id) {
      var model = new HomeModel();
      if (id.HasValue) {
        model.Vraag = ExpertVraagData.GetBySecurityToken(id.Value);  // Via security code wordt, on purpose, Zichtbaar omzeild
        if (model.Vraag != null)
          model.Antwoorden = model.Vraag.ExpertAntwoord.Where(ea => ea.TeltAntwoord).ToList();
      }
      return model;
    }

    public static HomeModel CreateAntwoordDetailsBekijken(Guid? id, Guid? antwoordID) {
      var model = CreateAntwoordenBekijken(id);
      if (model.Vraag != null) {
        model.Vraag = ExpertVraagData.GetBySecurityToken(id.Value); // Via security code wordt, on purpose, Zichtbaar omzeild
        model.Antwoord = model.Vraag.ExpertAntwoord.Where(ea => ea.TeltAntwoord).FirstOrDefault(ea => ea.ID == antwoordID);
        if (model.Antwoord != null)
          model.Expert = model.Antwoord.Adviseur;
        else
          model.Vraag = null;
      }
      return model;
    }

    public static HomeModel CreateExpert(string id, int? page) {
      var model = new HomeModel();
      model.Expert = AdviseurData.GetAdviseur(id);
      model.AntwoordenPagination = ExpertAntwoordData.GetOverzichtZichtbare(model.Activiteit, model.Expert).OrderByDescending(a => a.AanmaakDatum).AsPagination(page ?? 1, 10);
      return model;
    }
  }
}