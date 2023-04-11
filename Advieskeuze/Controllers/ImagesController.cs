using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.Util.ImageUploader;
using System;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  /// <summary>
  /// Toont plaatjes van de website uit de database.
  /// </summary>
  /// <remarks>
  /// Alleen nog relevant voor lokaal ontwikkelen. Normaal loopt dit via azure. Je wilt dit
  /// lokaal wel aan hebben omdat je lokaal de plaatjes naar lokaal upload en niet azure.
  /// </remarks>
  public class ImagesController : Controller {
    public ActionResult GetImage(string id, bool? isThumb) {
      try {
        var image = ImageHelper.GetImage(id, isThumb);
        return File(image.File, image.MimeType);
      }
      catch (Exception ex) {
        ErrorData.ElmahLog(ex);
      }
      return new HttpStatusCodeResult(404, "Image not found");
    }
  }
}
