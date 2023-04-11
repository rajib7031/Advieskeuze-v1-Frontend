using AdvieskeuzeCode.BI.Advieskeuze.BeoordelingModel;
using AdvieskeuzeCode.BI.Advieskeuze.VraagAntwoordModel;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Advieskeuze.Data.Domain.ViewModel.Beoordeling {
  public class BeoordelingBevestigen {
    public BeoordelingSecureEntity Link { get; set; }
    internal BeoordelingStatusHelper Beoordeling { get; set; }
    public bool MagBevestigingTonen { get; set; }
    public bool IsDemo { get; set; }
    internal IEnumerable<VraagAntwoordEnginePagina.Vraag> DemografieVragen { get; set; }
    public bool DemografieVragenStellen { get; set; }
    public DemografischeVragen DemografieData { get; set; }
    public IEnumerable<Bedankje> Bedankjes { get; set; }
    public bool BedankjesRelevant { get; set; }
    public bool BedankjeVraagStellen { get; set; }
    public CampagneActiviteit CampagneActiviteit { get; set; }

    public bool HasCustomLandingPage {
      get {
        return CampagneActiviteit != null && !string.IsNullOrEmpty(CampagneActiviteit.ReviewThanksContent) && !string.IsNullOrEmpty(CampagneActiviteit.ReviewThanksUrl);
      }
    }

    public class DemografischeVragen {
      public List<SelectListItem> BeoordelaarAanbiederList { get; set; }
      public IEnumerable<SelectListItem> EindeReview_Jul22_AfsluitList { get; set; }
      public IEnumerable<SelectListItem> EindeReview_Jul22_ProvisieList { get; set; }

      [Display(Name = "Als het gesprek over een product van een specifieke aanbieder is gegaan, welke aanbieder betrof het?")]
      public string BeoordelaarAanbieder { get; set; }

      [Display(Name = "Hoe ben je tot de keuze voor dit bedrijf en/of deze adviseur gekomen?")]
      public string EindeReview_Aug21_Adviseurkeuze { get; set; }

      [Display(Name = "Heb je tips voor de adviseur om hun dienstverlening te verbeteren?")]
      public string EindeReview_Aug21_Adviseurtips { get; set; }

      [Display(Name = "De volgende vragen zijn relevant als je een schadeverzekering (bijvoorbeeld woonverzekering) hebt afgesloten. Op welke wijze heb je de (meeste) schadeverzekeringen afgesloten?")]
      public string EindeReview_Jul22_Afsluitwijze { get; set; }

      [Display(Name = "Partijen die schadeverzekeringen verkopen worden op termijn verplicht om duidelijkheid te geven over hun verdiensten (provisie). Vind je dit belangrijk?")]
      public string EindeReview_Jul22_ProvisietransparantieBelangrijk { get; set; }
      [Display(Name = "Op welke wijze zou je het liefst geïnformeerd willen worden over de verdiensten (provisie) in jouw schadeverzekeringen?")]
      public string EindeReview_Jul22_Provisiewijze { get; set; }
    }

    public class Bedankje {
      public Guid ID { get; set; }
      public string Naam { get; set; }
      public string OrganisatieNaam { get; set; }
      public string ExtraVraag1 { get; set; }
      public bool ExtraVraag1Relevant => !string.IsNullOrEmpty(ExtraVraag1);
      public string ExtraVraag2 { get; set; }
      public bool ExtraVraag2Relevant => !string.IsNullOrEmpty(ExtraVraag2);
      public BedankRenderType RenderType { get; set; }
      public IEnumerable<Regel> Keuzes { get; set; }
      public BedankVraag BedankVraag { get; set; }

      public class Regel {
        public Guid ID { get; set; }
        public string Naam { get; set; }
        public IFotoBestandLink Logo { get; set; }
        public int SortIndex { get; set; }
        public string Slug { get; set; }
      }
    }
    public class BedankVraag {
      public bool ExtraVraag1 { get; set; }
      public bool ExtraVraag2 { get; set; }

      [Required(ErrorMessage = "Kies een cadeau")]
      public Guid? BedankKeuze { get; set; }
    }
  }
}