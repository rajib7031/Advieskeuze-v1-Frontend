using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  public class ErrorController : Controller {
    public ActionResult Error500() {
      Response.StatusCode = 500;
      Response.TrySkipIisCustomErrors = true;
      return View();
    }
    public ActionResult Error404() {
      Response.StatusCode = 404;
      Response.TrySkipIisCustomErrors = true;
      return View();
    }
  }
}
