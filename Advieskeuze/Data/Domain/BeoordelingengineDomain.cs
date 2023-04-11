using Advieskeuze.Data.Domain.ViewModel.Beoordeling;
using AdvieskeuzeCode;
using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.BI.Advieskeuze.VraagAntwoordModel;
using System;
using System.Collections.Generic;

namespace Advieskeuze.Data.Domain {
  public sealed class BeoordelingengineDomain {
    private readonly DataContextFactory _context;
    private readonly BeoordelingengineData.IEnvironment _environment;
    private readonly EnvironmentData.IEnvironmentTokenCampagne _environmentCampagne;
    private readonly RequestToken _request;
    private readonly string _moduleSlug;
    private BeoordelingengineData BeoordelingengineBI => new BeoordelingengineData(_context, _environment, _moduleSlug) { AutoSaveChanges = AutoSaveChanges, };
    private VraagAntwoordEngineData VraagAntwoordEngineBI => new VraagAntwoordEngineData(_context) { AutoSaveChanges = AutoSaveChanges, };
    public bool AutoSaveChanges { private get; set; }

    public BeoordelingengineDomain(DataContextFactory context, RequestToken request, BeoordelingengineData.IEnvironment environment, EnvironmentData.IEnvironmentTokenCampagne environmentCampagne, string moduleSlug) {
      if (context == null)
        throw new ArgumentNullException("context");
      if (environment == null)
        throw new ArgumentNullException("environment");
      _context = context;
      _environment = environment;
      _environmentCampagne = environmentCampagne;
      _request = request;
      _moduleSlug = moduleSlug;
      AutoSaveChanges = true;
    }

    /// <summary>
    /// Kijk of we de route kunnen starten vanuit de opgegeven parameters en of dit een valide beoordeling route wordt
    /// </summary>
    public BeoordelingengineData.BeoordelingEngineToken StartIndex(string postcodeKantoorStart, string kantoorSlugStart, string adviseurSlugStart, Guid? oudeBeoordelingID, bool? isDemo, int? gewenstePaginaNr, string klantemail, string klantnaam, Guid? companyId, int? waardering = null) {
      return BeoordelingengineBI.MaakToken(null, postcodeKantoorStart, kantoorSlugStart, adviseurSlugStart, oudeBeoordelingID, isDemo, gewenstePaginaNr, klantemail, klantnaam, companyId, waardering);
    }

    /// <summary>
    /// Ga verder met de route
    /// </summary>
    public BeoordelingengineData.BeoordelingEngineToken Start(Guid? sessieID, int? gewenstePaginaNr = null, bool isSubmit = false, bool isDemo = false) {
      return BeoordelingengineBI.Start(sessieID, _request.IP, gewenstePaginaNr, isSubmit, isDemo);
    }

    /// <summary>
    /// Update de pagina met live informatie
    /// </summary>
    public void UpdateWithLive(BeoordelingengineData.BeoordelingEngineToken engineToken, VraagAntwoordEnginePagina pagina) {
      BeoordelingengineBI.UpdateWithLive(engineToken, pagina);
    }

    /// <summary>
    /// Save changes aan de pagina
    /// </summary>
    public void SaveChanges(BeoordelingengineData.BeoordelingEngineToken engineToken, VraagAntwoordEnginePagina pagina) {
      BeoordelingengineBI.SaveChanges(engineToken, pagina);
    }

    /// <summary>
    /// Kijk of we naar de volgende pagina kunnen en wat we doen als overgangsstap
    /// </summary>
    public void VolgendePagina(BeoordelingengineData.BeoordelingEngineToken engineToken, VraagAntwoordEnginePagina pagina) {
      BeoordelingengineBI.VolgendePagina(engineToken, pagina, _request.IP);
    }

    public object GetReplaceVelden(BeoordelingengineData.BeoordelingEngineToken engineToken) {
      return BeoordelingengineBI.GetReplaceVelden(engineToken);
    }

    public BeoordelingVragenPagina RenderPagina(IVraagAntwoordEnginePaginaToken engineToken, string beoordeeldeEntiteit, bool isSubmit, object replaceVelden) {
      var model = new BeoordelingVragenPagina();
      model.Data = VraagAntwoordEngineBI.RenderPagina(engineToken, beoordeeldeEntiteit, isSubmit, replaceVelden);
      var ca = _environmentCampagne.CampagneActiviteit;
      model.GebruikPartialController = ca.GebruikPartialController();
      if (model.GebruikPartialController)
        model.GebruikPartialNaamDeel = ca.Data.GebruikPartialNaamDeel;
      model.BedankjesOrganisaties = model.Data.PaginaNr == 1 ? BeoordelingengineBI.GetBedankjes(_environmentCampagne) : new List<string>();
      return model;
    }

    public void PatchLiveWithBinded(IVraagAntwoordEnginePaginaToken engineToken, VraagAntwoordEnginePagina paginaLive, VraagAntwoordEnginePagina paginaBinded) {
      VraagAntwoordEngineBI.PatchLiveWithBinded(engineToken, paginaLive, paginaBinded);
    }
  }
}
