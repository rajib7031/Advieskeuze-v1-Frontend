using AdvieskeuzeCode.DataModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Advieskeuze.Models {
  public class BeoordelingBedanktModel {
    public static BeoordelingBedanktModel Create(Guid id) {
      var model = new BeoordelingBedanktModel();
      model.Beoordeling = AdvieskeuzeCode.Data.BeoordelingData.Get(id);
      if (model.Beoordeling != null) {
        model.Emailadres = model.Beoordeling.Emailadres;
        model.OpnieuwVerstuurd = false;
      }
      return model;
    }
    public Beoordeling Beoordeling { get; set; }

    public bool OpnieuwVerstuurd { get; set; }

    [Display(Name = "Email", ResourceType = typeof(Resources.Messages))]
    [Required(ErrorMessageResourceType = typeof(Resources.Messages), ErrorMessageResourceName = "EmailRequired")]
    [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resources.Messages), ErrorMessageResourceName = "EmailInvalid")]
    public string Emailadres { get; set; }
  }
}