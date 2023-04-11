using Advieskeuze.Data.Domain.ViewModel.Beoordeling;
using AdvieskeuzeCode;
using AdvieskeuzeCode.Azure.Storage;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.BI.Shared;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Mvc;
using AdvieskeuzeCode.Social.Emails;
using AdvieskeuzeCode.Util.BeoordelingHelper;
using Mandrill.Models;
using SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// Domain voor beoordelingen controller op hoofdniveau
  /// </summary>
  public sealed class BeoordelingDomain {
    private readonly DataContextFactory _context;
    private readonly EnvironmentDomain.EnvironmentToken _environment;
    private readonly RequestToken _request;
    private BeoordelingData BeoordelingBI => new BeoordelingData(_context);
    private CacheData CacheBI => new CacheData(_context);

    public BeoordelingDomain(DataContextFactory context, RequestToken request, EnvironmentDomain.EnvironmentToken environment) {
      if (context == null)
        throw new ArgumentNullException("context");
      if (environment == null)
        throw new ArgumentNullException("environment");
      _context = context;
      _environment = environment;
      _request = request;
    }

    public BeoordelingBevestigen StartBevestigen(Guid beoordelingSecurityID) {
      const int demografieVragenPagina = 6;
      var model = new BeoordelingBevestigen();
      model.Beoordeling = BeoordelingBI.GetViaSecurityCode(beoordelingSecurityID);
      if (model.Beoordeling == null)
        return model;
      model.Link = new BeoordelingSecureEntity() { ID = model.Beoordeling.ID, SecurityID = beoordelingSecurityID };
      // Je mag de bevestigen pagina zien als:
      // -7 dagen na bevestigen (en/of goedkeuren)
      // -De demografievragen al zijn gesteld en het bedankje al is geselecteerd (per item geld een afzonderlijke controle)
      // Je mag de bevestigen pagina niet zien als:
      // -Je de securitycode niet goed hebt
      // -De beoordeling afgekeurd is, in toelichting en in onderzoek zit
      model.MagBevestigingTonen = (new[] { (int)BeoordelingStatus.KlantBevestigd, (int)BeoordelingStatus.Goedgekeurd }.Contains(model.Beoordeling.StatusCode) && model.Beoordeling.Datum > DateTime.Now.Local().AddDays(-7));
      if (!model.MagBevestigingTonen)
        return model;
      model.IsDemo = model.Beoordeling.IsDemo;

      // Zet campagneactiviteit live die ook in de beoordeling zit
      if (!string.IsNullOrEmpty(model.Beoordeling.CampagneActiviteitExternID)) {
        _environment.CampagneActiviteit.Set(new EnvironmentData(_context), model.Beoordeling.CampagneActiviteitExternID);
        model.CampagneActiviteit = AdvieskeuzeCode.Data.CampagneActiviteitData.GetCampagneActiviteitFromExtern(model.Beoordeling.CampagneActiviteitExternID);
      }

      // demografie vragen regelen
      var engine = new BeoordelingengineData(_context, new BeoordelingengineData.EnvironmentWrapper<EnvironmentDomain.EnvironmentToken>(_context, _environment), model.Beoordeling.BeoordelingmoduleSlug);
      var vraagAntwoordBI = new VraagAntwoordEngineData(_context);
      model.DemografieVragen = vraagAntwoordBI.KrijgLosseVragen(engine.GetPagina(demografieVragenPagina).Value);
      engine.UpdateWithLive(null, model.DemografieVragen, model.Beoordeling.Pagina6Antwoorden, null, null);
      model.DemografieVragenStellen = model.DemografieVragen.All(v => v.GetAntwoordRaw() == null);
      if (model.DemografieVragenStellen) {
        model.DemografieData = new BeoordelingBevestigen.DemografischeVragen();
        // Preselect de aanbiederlijst op de scope vd beoordeling
        model.DemografieData.BeoordelaarAanbiederList = SelectListFactory.CreateAanbiederProductgroepLijst(model.Beoordeling.ScopeSlug, null, null, false).ToList();
        model.DemografieData.BeoordelaarAanbiederList.Insert(0, (new SelectListItem { Text = "--", Value = Guid.Empty.ToString() }));
        if (model.DemografieData.BeoordelaarAanbiederList.Count == 1) // 1 == leeg
          model.DemografieData.BeoordelaarAanbiederList.Insert(1, (new SelectListItem { Text = "Niet van toepassing", Value = Guid.Empty.ToString() }));
        model.DemografieData.EindeReview_Jul22_AfsluitList = new SelectList(new SelectListFactoryCached(_context).GeefAntwoordLijstCode(202), "Key", "Text");
        model.DemografieData.EindeReview_Jul22_ProvisieList = new SelectList(new SelectListFactoryCached(_context).GeefAntwoordLijstCode(203), "Key", "Text");
      }

      // bedankje vragen
      var organisatie = new OrganisatieEntity { ID = model.Beoordeling.CampagneOrganisatie };
      var bedankVragen = BeoordelingBI.Bedankjes(model.Beoordeling.BeoordelingmoduleSlug, organisatie)
        .Where(b => b.Regels.Any()).ToList();  // alleen bedankjes die 1 of meer keuze items hebben
      model.BedankjesRelevant = bedankVragen.Any();
      model.BedankjeVraagStellen = model.BedankjesRelevant && !model.Beoordeling.BedankjeGekozen;
      if (model.BedankjeVraagStellen) {
        model.Bedankjes = bedankVragen.Select(bv => new BeoordelingBevestigen.Bedankje() {
          ID = bv.BedankID,
          Naam = bv.Naam,
          OrganisatieNaam = bv.OrganisatieNaam,
          ExtraVraag1 = bv.ExtraVraag1,
          ExtraVraag2 = bv.ExtraVraag2,
          BedankVraag = new BeoordelingBevestigen.BedankVraag(),
          RenderType = (BedankRenderType)bv.PresentatieTypeCode,
          Keuzes = bv.Regels.Select(r => new BeoordelingBevestigen.Bedankje.Regel() {
            ID = r.ID,
            Naam = r.Naam,
            SortIndex = r.SortIndex,
            Logo = r.LogoID.HasValue ? new FotoBestand() { ID = r.LogoID.Value, Extensie = r.LogoExtensie, } : null,
            Slug = r.Slug,
          }).ToList(),
        }).ToList();
      }
      return model;
    }

    public void DoeBevestigen(BeoordelingBevestigen model) {
      if (model.IsDemo)
        // De status van de beoordeling aanpassen. Demo beoordelingen worden niet (in tegenstelling tot wat de gebruiker wordt gezegd) verwijdert.
        BeoordelingBI.AfkeurenDemo(model.Link, new BeoordelingUpdateData(_request.IP));
      else
        BeoordelingBI.ZetBevestigd(model.Link, new BeoordelingUpdateData(_request.IP));
    }

    public void DemografieOpslaan(BeoordelingBevestigen model, BeoordelingBevestigen.DemografischeVragen validatedVragen) {
      if (model.IsDemo || !model.DemografieVragenStellen)
        return;

      var BeoordelaarAanbiederVraag = model.DemografieVragen.FirstOrDefault(v => String.Equals(v.Key, "BeoordelaarAanbieder", StringComparison.CurrentCultureIgnoreCase));
      if (BeoordelaarAanbiederVraag != null && !string.IsNullOrEmpty(validatedVragen.BeoordelaarAanbieder) && !validatedVragen.BeoordelaarAanbieder.Contains("00000000"))
        BeoordelaarAanbiederVraag.ValueGuid = Guid.Parse(validatedVragen.BeoordelaarAanbieder);

      var EindeReview_Aug21_Adviseurkeuze = model.DemografieVragen.FirstOrDefault(v => string.Equals(v.Key, "EindeReview_Aug21_Adviseurkeuze", StringComparison.CurrentCultureIgnoreCase));
      if (EindeReview_Aug21_Adviseurkeuze != null && !string.IsNullOrEmpty(validatedVragen.EindeReview_Aug21_Adviseurkeuze))
        EindeReview_Aug21_Adviseurkeuze.ValueString = validatedVragen.EindeReview_Aug21_Adviseurkeuze;

      var EindeReview_Aug21_Adviseurtips = model.DemografieVragen.FirstOrDefault(v => string.Equals(v.Key, "EindeReview_Aug21_Adviseurtips", StringComparison.CurrentCultureIgnoreCase));
      if (EindeReview_Aug21_Adviseurtips != null && !string.IsNullOrEmpty(validatedVragen.EindeReview_Aug21_Adviseurtips))
        EindeReview_Aug21_Adviseurtips.ValueString = validatedVragen.EindeReview_Aug21_Adviseurtips;

      var EindeReview_Jul22_Afsluitwijze = model.DemografieVragen.FirstOrDefault(v => string.Equals(v.Key, "EindeReview_Jul22_Afsluitwijze", StringComparison.CurrentCultureIgnoreCase));
      if (EindeReview_Jul22_Afsluitwijze != null && !string.IsNullOrEmpty(validatedVragen.EindeReview_Jul22_Afsluitwijze))
        EindeReview_Jul22_Afsluitwijze.ValueString = validatedVragen.EindeReview_Jul22_Afsluitwijze;

      var EindeReview_Jul22_ProvisietransparantieBelangrijk = model.DemografieVragen.FirstOrDefault(v => string.Equals(v.Key, "EindeReview_Jul22_ProvisietransparantieBelangrijk", StringComparison.CurrentCultureIgnoreCase));
      if (EindeReview_Jul22_ProvisietransparantieBelangrijk != null && !string.IsNullOrEmpty(validatedVragen.EindeReview_Jul22_ProvisietransparantieBelangrijk))
        EindeReview_Jul22_ProvisietransparantieBelangrijk.ValueString = validatedVragen.EindeReview_Jul22_ProvisietransparantieBelangrijk;

      var EindeReview_Jul22_Provisiewijze = model.DemografieVragen.FirstOrDefault(v => string.Equals(v.Key, "EindeReview_Jul22_Provisiewijze", StringComparison.CurrentCultureIgnoreCase));
      if (EindeReview_Jul22_Provisiewijze != null && !string.IsNullOrEmpty(validatedVragen.EindeReview_Jul22_Provisiewijze))
        EindeReview_Jul22_Provisiewijze.ValueString = validatedVragen.EindeReview_Jul22_Provisiewijze;

      var engine = new BeoordelingengineData(_context, new BeoordelingengineData.EnvironmentWrapper<EnvironmentDomain.EnvironmentToken>(_context, _environment), model.Beoordeling.BeoordelingmoduleSlug);
      var vraagAntwoordBI = new VraagAntwoordEngineData(_context);
      engine.SaveChangesFindBeoordeling(model.Beoordeling, model.DemografieVragen, model.Beoordeling.Pagina6Antwoorden);
    }

    public void BedankjeKiezen(BeoordelingBevestigen model, BeoordelingBevestigen.Bedankje validatedBedankje) {
      if (model.IsDemo || !model.BedankjeVraagStellen) {
        return;
      }

      var organisatie = new OrganisatieEntity { ID = model.Beoordeling.CampagneOrganisatie };
      var bedankVragen = BeoordelingBI.Bedankjes(model.Beoordeling.BeoordelingmoduleSlug, organisatie);
      string pdf = string.Empty;
      if (validatedBedankje.BedankVraag.BedankKeuze.HasValue) {
        var geselecteerdItem = bedankVragen.SelectMany(v => v.Regels).SingleOrDefault(r => r.ID == validatedBedankje.BedankVraag.BedankKeuze.Value);
        pdf = geselecteerdItem.StorePDF;
      }

      var emailBeoordeling = BeoordelingBI.VoorEmail(model.Beoordeling);
      var engine = new BeoordelingengineData(_context, new BeoordelingengineData.EnvironmentWrapper<EnvironmentDomain.EnvironmentToken>(_context, _environment), model.Beoordeling.BeoordelingmoduleSlug);
      engine.SaveBedanktItem(model.Beoordeling, new AdvieskeuzeCode.BI.Advieskeuze.BeoordelingModel.BedankVraag {
        BedankItem = validatedBedankje.BedankVraag.BedankKeuze.Value,
        ExtraVraag1 = validatedBedankje.BedankVraag.ExtraVraag1,
        ExtraVraag2 = validatedBedankje.BedankVraag.ExtraVraag2,
      });

      if (!string.IsNullOrEmpty(pdf)) {
        // Download PDF from Azure & send email in a background task
        System.Threading.Tasks.Task.Factory.StartNew(state => {
          dynamic s = (dynamic)state;
          this.SendEmail(s.emailBeoordeling as AdvieskeuzeCode.BI.Common.BeoordelingModel.BeoordelingEmail, s.pdf);
        }, new {
          emailBeoordeling,
          pdf
        }).ContinueWith(t => {
          if (t.IsFaulted && t.Exception != null) {
            AdvieskeuzeCode.Util.Logger.Log(t.Exception.InnerException);
          }
        });
      }
    }

    private void SendEmail(AdvieskeuzeCode.BI.Common.BeoordelingModel.BeoordelingEmail beoordeling, string fileName) {
      var storageContainer = StorageFactory.Current.GetOrCreatePermanentContainer(StoreType.Pdf);
      if (!storageContainer.ItemExists(fileName)) {
        throw new System.IO.FileNotFoundException($"Kon bestand [{fileName}] niet vinden in de StorageContainer");
      }

      var filestream = storageContainer.ReadFromItem(fileName);
      EmailAttachment bijlage = null;
      if (filestream != null) {
        bijlage = new EmailAttachment();
        Int32 strLen, strRead;
        strLen = Convert.ToInt32(filestream.Length);
        byte[] strArr = new byte[strLen];
        strRead = filestream.Read(strArr, 0, strLen);
        bijlage.Content = Convert.ToBase64String(strArr);
        bijlage.Base64 = true;
        bijlage.Name = fileName;
        bijlage.Type = System.Net.Mime.MediaTypeNames.Application.Pdf;
      }
      var argDict = new Dictionary<string, object>();
      argDict.Add("AanhefNaam", beoordeling.AanhefNaam);
      List<string> emailadressen = new List<string>();
      emailadressen.Add(beoordeling.Emailadres);
      System.Threading.Tasks.Task.Run(() =>
        MandrillEmailHelper.SendTemplateAsync(emailadressen, EmailSender.AdvieskeuzeInfo.Address, EmailSender.AdvieskeuzeInfo.DisplayName,
        argDict, MandrillEmailTemplates.BedankjeVerstuurd, RequestToken.FallbackIPAdres, "Bedankje verstuurd", attachment: bijlage
      ));

    }
  }
}