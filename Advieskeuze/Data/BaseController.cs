using Advieskeuze.Data.Domain;
using AdvieskeuzeCode;
using SharedCode;
using System.Web.Mvc;

namespace Advieskeuze.Data {
  public abstract class BaseController : Controller {
    public AdvieskeuzeDomain DomainContext { get; private set; }
    public EnvironmentDomain.EnvironmentToken Environment => DomainContext.Environment;
    public SitemapWrapper SiteMap { get; private set; }
    internal RequestToken RequestUtil { get; set; }

    public BaseController() {
      SiteMap = new SitemapWrapper();
    }

    internal void ApplyContext(AdvieskeuzeDomain domainContext) {
      DomainContext = domainContext;
    }

    public void LaadEnvironment(string routeArea, string routeAreaAdvies, string routeAreaProductgroep, string routeThema, string routeProductgroep, string routeBeoordelingmodule, string querystringCCode, string postCCode) {
      var environmentDomain = DomainContext.GetEnvironment();
      environmentDomain.ApplyBeoordelingmodule(Environment, routeBeoordelingmodule);
      environmentDomain.ApplyProductgroep(Environment, routeAreaAdvies, new string[] { routeArea, routeAreaProductgroep, routeProductgroep });
      environmentDomain.ApplyCampagneActiviteit(Environment, new string[] { postCCode, querystringCCode });
    }

    public void SetCacheWarning() {
      var environmentDomain = DomainContext.GetEnvironment();
      environmentDomain.ApplyCampagneActiviteitCacheWarning(Environment);
    }
    public void SetScope(AdvieskeuzeCode.DataModel.CampagneActiviteitScope scope) {
      Environment.CampagneActiviteit.Scope = scope;
    }
  }
}