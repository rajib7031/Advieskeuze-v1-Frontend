using AdvieskeuzeCode.BI;
using AdvieskeuzeCode.BI.Advieskeuze;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System;

namespace Advieskeuze.Data.Domain {
  /// <summary>
  /// This handles some environmental settings used by www.advieskeuze.nl.
  /// It sets data regarding scope and reviewmodule that are active based on the visited URL.
  /// </summary>
  public sealed class EnvironmentDomain {
    private readonly DataContextFactory _context;
    private EnvironmentData EnvironmentBI => new EnvironmentData(_context);

    public EnvironmentDomain(DataContextFactory context) {
      if (context == null)
        throw new ArgumentNullException("context");
      _context = context;
    }

    public EnvironmentToken GetNewToken() {
      return new EnvironmentToken();
    }

    public void ApplyProductgroep(EnvironmentToken opToken, string slugAdvies, string[] productgroepSlugs) {
      EnvironmentBI.ApplyProductgroep(opToken, slugAdvies, productgroepSlugs);
    }

    public void ApplyBeoordelingmodule(EnvironmentToken opToken, string beoordelingmoduleSlug) {
      EnvironmentBI.ApplyBeoordelingmodule(opToken, beoordelingmoduleSlug);
    }

    public void ApplyCampagneActiviteit(EnvironmentData.IEnvironmentTokenCampagne opToken, string[] cCodes) {
      EnvironmentBI.ApplyCampagneActiviteit(opToken, cCodes);
    }
    public void ApplyCampagneActiviteitCacheWarning(EnvironmentData.IEnvironmentTokenCampagne opToken) {
      opToken.CampagneActiviteit.CacheWarning = true;
    }
    public void ApplyCampagneActiviteit(EnvironmentData.IEnvironmentTokenCampagne opToken, string cCode) {
      EnvironmentBI.ApplyCampagneActiviteit(opToken, new string[] { cCode });
    }

    public class EnvironmentToken : EnvironmentData.IEnvironmentFull {
      public Beoordelingmodule Beoordelingmodule { get; set; }
      public bool HeeftBeoordelingmodule { get; set; }
      public IProductgroepCache Productgroep { get; set; }
      public bool HeeftProductgroep => Productgroep != null;
      public CampagneActiviteitHelper CampagneActiviteit { get; private set; }
      public bool HeeftCampagneActiviteit => CampagneActiviteit.GebruikQueryString();

      public EnvironmentToken() {
        CampagneActiviteit = new CampagneActiviteitHelper();
      }
    }
  }
}
