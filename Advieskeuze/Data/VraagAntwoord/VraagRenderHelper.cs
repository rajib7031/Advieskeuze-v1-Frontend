using AdvieskeuzeCode.BI.Advieskeuze.VraagAntwoordModel;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using SharedCode;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Advieskeuze.Data.VraagAntwoord {
  public class VraagRenderHelper<T> where T : VraagAntwoordEnginePagina.Vraag {
    private readonly VraagAntwoordEnginePagina.Vraag _vraag;
    private readonly ViewDataDictionary<T> _viewContext;

    private readonly string[,] _labels = { { "zeer slecht", "zeer goed" }, { "zeer onwaarschijnlijk", "zeer waarschijnlijk" } };

    public string RadioLabelLinks {
      get {
        if (IsScore)
          return _vraag.AntwoordLijst == (int)VraagAntwoordLijstCode.Score0Tot10 ? _labels[1, 0] : _labels[0, 0];
        return "";
      }
    }

    public string RadioLabelRechts {
      get {
        if (IsScore)
          return _vraag.AntwoordLijst == (int)VraagAntwoordLijstCode.Score0Tot10 ? _labels[1, 1] : _labels[0, 1];
        return "";
      }
    }

    public bool ISScore10 {
      get {
        if (IsScore)
          return _vraag.AntwoordLijst == (int)VraagAntwoordLijstCode.Score0Tot10;
        return false;
      }
    }

    public bool HeeftAntwoord => _vraag.GetAntwoordAsString().IsNotNullOrEmpty();

    private IProductgroepCache _engineProductgroep;
    public IProductgroepCache EngineProductgroep {
      get {
        if (_engineProductgroep == null)
          _engineProductgroep = _viewContext["EngineProductgroep"] as IProductgroepCache;
        return _engineProductgroep;
      }
    }

    private Guid? _companyId;
    public Guid? CompanyId {
      get {
        if (_companyId == null)
          _companyId = _viewContext["CompanyId"] as Guid?;
        return _companyId;
      }
    }

    public bool ListGuid => (_vraag.EntityType.HasValue) && _vraag.DataType == VraagType.GUID;

    public bool ListString => (_vraag.EntityType.HasValue || _vraag.AntwoordLijst.HasValue) && _vraag.DataType == VraagType.Tekst;

    public bool ListInt => _vraag.AntwoordLijst.HasValue && new[] { VraagType.Nummer, VraagType.Boolean }.Contains(_vraag.DataType);

    public bool ListMulti => _vraag.AntwoordLijst.HasValue && _vraag.DataType == VraagType.MultiNummer;

    public bool IsScore => _vraag.AntwoordLijst.HasValue && new[] { (int)VraagAntwoordLijstCode.Score1Tot5, (int)VraagAntwoordLijstCode.Score0Tot10, (int)VraagAntwoordLijstCode.Score1Tot10 }.Contains(_vraag.AntwoordLijst.Value);

    public bool IsEensOneens => _vraag.AntwoordLijst.HasValue && new[] { (int)VraagAntwoordLijstCode.KTOEensOneens }.Contains(_vraag.AntwoordLijst.Value);

    public VraagRenderHelper(T vraag, ViewDataDictionary<T> context) {
      _vraag = vraag;
      _viewContext = context;
    }

    public string GetName() {
      return _viewContext.TemplateInfo.GetFullHtmlFieldName(_vraag.GetAntwoordFieldName());
    }
    public string GetID() {
      return _viewContext.TemplateInfo.GetFullHtmlFieldId(_vraag.GetAntwoordFieldName());
    }

    public string GetVerwijzingName() {
      if (!_vraag.IsAfhankelijk)
        return null;
      return Regex.Replace(_viewContext.TemplateInfo.GetFullHtmlFieldName(_vraag.AfhankelijkVraag.VeldNaam),
        @"VragenLos\[\d*]", $"VragenLos[{_vraag.AfhankelijkVraag.VeldIndex}]");
    }
    public string GetVerwijzingID() {
      if (!_vraag.IsAfhankelijk)
        return null;
      return GetVerwijzingName().ToValidHtmlID();
    }
  }
}