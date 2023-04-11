using Advieskeuze.Data.Domain;
using SharedCode;
using System.Web.Mvc;

namespace Advieskeuze.Data {
  public class CompositionRoot {
    private readonly IControllerFactory _controllerFactory;

    public CompositionRoot() {
      _controllerFactory = CreateControllerFactory();
    }

    public IControllerFactory ControllerFactory => _controllerFactory;

    private static IControllerFactory CreateControllerFactory() {
      var repositoryFactory = new AdvieskeuzeDomainFactory(GewrapteSettings.ConnectionstringEntityFramework);
      return new BaseControllerFactory(repositoryFactory);
    }
  }
}