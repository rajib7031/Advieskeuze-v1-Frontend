using AdvieskeuzeCode.BI.Advieskeuze.VraagAntwoordModel;
using System.Collections.Generic;

namespace Advieskeuze.Data.Domain.ViewModel.Beoordeling {
  public class BeoordelingVragenPagina {
    public VraagAntwoordEnginePagina Data { get; set; }
    public bool GebruikPartialController { get; set; }
    public string GebruikPartialNaamDeel { get; set; }
    public IEnumerable<string> BedankjesOrganisaties { get; set; }
  }
}