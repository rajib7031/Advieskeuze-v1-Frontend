using AdvieskeuzeCode;
using AdvieskeuzeCode.BI;

namespace Advieskeuze.Data.Domain {
  public sealed class AdvieskeuzeDomainFactory {
    private string _connection { get; set; }

    public AdvieskeuzeDomainFactory(string entityFrameworkDbConnection) {
      _connection = entityFrameworkDbConnection;
    }

    public AdvieskeuzeDomain GetNewRepository(RequestToken request) {
      return new AdvieskeuzeDomain(new DataContextFactory(_connection), request);
    }
  }

  public sealed class AdvieskeuzeDomain {
    private readonly DataContextFactory _context;
    private readonly RequestToken _request;
    private EnvironmentDomain.EnvironmentToken _environment;
    public EnvironmentDomain.EnvironmentToken Environment {
      get {
        if (_environment == null)
          _environment = GetEnvironment().GetNewToken();
        return _environment;
      }
    }

    public AdvieskeuzeDomain(DataContextFactory dataContext, RequestToken request) {
      _context = dataContext;
      _request = request;
    }

    public BeoordelingengineDomain GetBeoordelingengine(string moduleSlug) {
      return new BeoordelingengineDomain(_context, _request, new AdvieskeuzeCode.BI.Advieskeuze.BeoordelingengineData.EnvironmentWrapper<EnvironmentDomain.EnvironmentToken>(_context, Environment), Environment, moduleSlug);
    }
    public EnvironmentDomain GetEnvironment() {
      return new EnvironmentDomain(_context);
    }
    public HomeDomain GetHome() {
      return new HomeDomain(_context, Environment);
    }
    public LocatiesDomain GetLocaties() {
      return new LocatiesDomain(_context, _request, Environment);
    }
    public OverDomain GetOver() {
      return new OverDomain(_context);
    }
    public BeoordelingDomain GetBeoordeling() {
      return new BeoordelingDomain(_context, _request, Environment);
    }
    public BannerDomain GetBanner() {
      return new BannerDomain(_context, _request, Environment);
    }
  }
}
