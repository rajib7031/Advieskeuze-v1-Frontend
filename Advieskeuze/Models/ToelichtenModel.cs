using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Models;
using AdvieskeuzeCode.Social.Emails;
using System;
using System.Linq;

namespace Advieskeuze.Models {
  public class ToelichtenModel {
    public Beoordeling Beoordeling { get; set; }
    public ExpertVraag Vraag { get; set; }
    public Email Email { get; set; }
    public EmailComposeModel Reactie { get; set; }
    public string OudeBeschrijving { get; set; }
    public Moderator NaarModerator { get; set; }

    public ToelichtenModel() {
      Reactie = new EmailComposeModel();
      Reactie.Van = EmailSender.AdvieskeuzeInfo.Address;
    }

    public static async System.Threading.Tasks.Task<ToelichtenModel> CreateBeoordeling(Guid id) {
      var model = new ToelichtenModel();
      model.Beoordeling = BeoordelingData.Get(id);
      if (model.Beoordeling == null || model.Beoordeling.Status != BeoordelingStatus.Toelichting) {
        model.Beoordeling = null;
        return model;
      }
      var moderating = model.Beoordeling.BeoordelingUpdate.OrderByDescending(m => m.Datum).FirstOrDefault(m => m.Status == (int)BeoordelingUpdateType.ToelichtingGevraagd);
      if (moderating == null) {
        model.Beoordeling = null;
        return model;
      }
      model.NaarModerator = model.Beoordeling.ModeratorID.HasValue ? ModeratorData.GetModerator(model.Beoordeling.ModeratorID.Value) : null;
      if (model.NaarModerator == null)
        model.NaarModerator = moderating.Moderator;
      if (moderating.EmailEventId.HasValue) {
        var emailEvent = EmailEventData.GetEmailEvent(moderating.EmailEventId.Value);
        var mandrillEmail = await MandrillEmailHelper.GetEmail(Guid.Parse(emailEvent.MandrillID));
        if (mandrillEmail != null)
          model.Email = mandrillEmail;
      }
      return model;
    }
  }
}