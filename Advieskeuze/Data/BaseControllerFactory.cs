using Advieskeuze.Data.Domain;
using AdvieskeuzeCode;
using System.Web.Mvc;
using System.Web.Routing;

namespace Advieskeuze.Data {
  /// <summary>
  /// Alle moderatie controllers lopen via deze factory
  /// </summary>
  /// <remarks>
  /// Nu even hardcoded nieuwe context en geen constructor. Mooier is http://www.dotnetcurry.com/showarticle.aspx?ID=786, dit is ff practischer.
  /// </remarks>
  public class BaseControllerFactory : DefaultControllerFactory {
    private readonly AdvieskeuzeDomainFactory _repositoryFactory;

    public BaseControllerFactory(AdvieskeuzeDomainFactory repositoryFactory) {
      _repositoryFactory = repositoryFactory;
    }

    public override IController CreateController(RequestContext requestContext, string controllerName) {
      var controller = base.CreateController(requestContext, controllerName);
      var baseController = controller as BaseController;
      if (baseController != null) {
        var requestUtil = new RequestToken(requestContext.HttpContext);
        baseController.RequestUtil = requestUtil;
        baseController.ApplyContext(_repositoryFactory.GetNewRepository(requestUtil));
      }
      return controller;
    }
  }
}