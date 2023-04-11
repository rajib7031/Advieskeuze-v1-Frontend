using Advieskeuze.Data;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using SharedCode;
using System;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class AdviesgesprekController : BaseController {
    public ActionResult WelContact(Guid? id) {
      if (!id.HasValue)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      var adviesgesprek = AdviesgesprekData.Get(id.Value);
      if (adviesgesprek == null || adviesgesprek.KlantReactieType != null || adviesgesprek.Datum < DateTime.Now.Local().AddMonths(-6))
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      var locatie = adviesgesprek.Locatie;
      if (!(locatie.Status == LocatieStatus.Actief || locatie.Status == LocatieStatus.Vrij))
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      adviesgesprek.KlantReactieType = (int)KlantReactieType.WelContact;
      DataContext.Current.SaveChanges();
      return View(locatie);
    }
    public ActionResult GeenContact(Guid? id) {
      if (!id.HasValue)
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      var adviesgesprek = AdviesgesprekData.Get(id.Value);
      if (adviesgesprek == null || adviesgesprek.KlantReactieType != null || adviesgesprek.Datum < DateTime.Now.Local().AddMonths(-6))
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      var locatie = adviesgesprek.Locatie;
      if (!(locatie.Status == LocatieStatus.Actief || locatie.Status == LocatieStatus.Vrij))
        return RedirectToRoute(AdvieskeuzeRoutes.Home());
      adviesgesprek.KlantReactieType = (int)KlantReactieType.GeenContact;
      DataContext.Current.SaveChanges();
      return View(locatie);
    }
  }
}
