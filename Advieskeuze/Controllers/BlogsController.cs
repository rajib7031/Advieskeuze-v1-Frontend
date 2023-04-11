using Advieskeuze.Data;
using Advieskeuze.Models;
using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  [AdvieskeuzeEnvironment]
  [SetCampagneActiviteitScope(CampagneActiviteitScope.Home)]
  public class BlogsController : BaseController {
    public ActionResult Index() {
      var model = new BlogIndexModel();
      model.Blogs = BlogData.GetBlogs(true, true, true).Take(3).ToList();
      return View(model);
    }
    [HttpPost]
    public ActionResult Index(FormCollection form) {
      var model = new BlogIndexModel();
      model.Blogs = BlogData.GetBlogs(true, true, true).Take(3).ToList();
      if (TryUpdateModel(model)) {
        if (!String.IsNullOrEmpty(model.Zoektermen)) {
          return RedirectToRoute(AdvieskeuzeRoutes.BlogsOverzicht(null, model.Zoektermen));
        }
      }
      return View(model);
    }
    public ActionResult Categorie(string id) {
      var model = new BlogcategorieModel();
      model.Blogcategorie = BlogcategorieData.GetBlogcategorie(id);
      if (model.Blogcategorie == null)
        return RedirectToRoute(AdvieskeuzeRoutes.BlogsOverzicht(null, null));
      model.Blogs = BlogData.GetBlogsMetCategorie(model.Blogcategorie.ID).Take(12).ToList();
      SiteMap.ObjectForFormatting = new { titel = model.Blogcategorie.Titel, omschrijving = model.Blogcategorie.Omschrijving };
      return View(model);
    }
    [HttpPost]
    public ActionResult Categorie(string id, FormCollection form) {
      var model = new BlogcategorieModel();
      model.Blogcategorie = BlogcategorieData.GetBlogcategorie(id);
      model.Blogs = BlogData.GetBlogsMetCategorie(model.Blogcategorie.ID).Take(12).ToList();
      SiteMap.ObjectForFormatting = new { titel = model.Blogcategorie.Titel, omschrijving = model.Blogcategorie.Omschrijving };
      if (TryUpdateModel(model)) {
        if (!String.IsNullOrEmpty(model.Zoektermen)) {
          return RedirectToRoute(AdvieskeuzeRoutes.BlogsOverzicht(id, model.Zoektermen));
        }
      }
      return View(model);
    }
    public ActionResult Overzicht(string id, string zoektermen) {
      var model = new BlogcategorieModel();
      var blogs = new List<Blog>();
      if (!String.IsNullOrEmpty(id)) {
        model.Blogcategorie = BlogcategorieData.GetBlogcategorie(id);
        if (model.Blogcategorie == null)
          return RedirectToRoute(AdvieskeuzeRoutes.BlogsOverzicht(null, null));
        blogs = BlogData.GetBlogsMetCategorie(model.Blogcategorie.ID).ToList();
      }
      else
        blogs = BlogData.GetBlogs(true, true, true).ToList();
      if (!String.IsNullOrEmpty(zoektermen)) {
        model.Zoektermen = zoektermen;
        var zoektermArray = zoektermen.Split(' ');
        foreach (var zoekterm in zoektermArray) {
          var matchingBlogs = blogs.Where(b => b.Tekst.ToLower().Contains(zoekterm.ToLower() + " ")
                || b.Tekst.ToLower().Contains(zoekterm.ToLower() + ".")
                || b.Tekst.ToLower().Contains(zoekterm.ToLower() + ",")
                || b.Titel.ToLower().Contains(zoekterm.ToLower() + " ")
                || b.Titel.ToLower().Contains(zoekterm.ToLower() + ".")
                || b.Titel.ToLower().Contains(zoekterm.ToLower() + ","));
          model.Blogs.AddRange(matchingBlogs);
        }
        model.Blogs = model.Blogs.Distinct().ToList();
      }
      else {
        model.Blogs = blogs;
      }
      return View(model);
    }
    [HttpPost]
    public ActionResult Overzicht(string id, string zoektermen, FormCollection form) {
      var model = new BlogcategorieModel();
      model.Blogs = BlogData.GetBlogs(true, true, true).Take(3).ToList();
      if (TryUpdateModel(model)) {
        if (!String.IsNullOrEmpty(model.Zoektermen)) {
          return RedirectToRoute(AdvieskeuzeRoutes.BlogsOverzicht(id, model.Zoektermen));
        }
      }
      return View(model);
    }
    public ActionResult Lezen(string id) {
      var model = BlogData.Get(id);
      if (model == null || !model.IsGepubliceerd || !model.IsGoedgekeurd)
        return RedirectToRoute(AdvieskeuzeRoutes.Blogs());
      SiteMap.ObjectForFormatting = new { auteur = model.Auteur + " (" + model.Datum.ToShortDateString() + ")", titel = model.Titel };
      BlogTrafficData.Log(RequestUtil, model.ID, true, false);
      return View(model);
    }

    public ActionResult RedirectNaarKantoor(string id) {
      var model = BlogData.Get(id);
      if (model == null || !model.IsGepubliceerd || !model.IsGoedgekeurd)
        return RedirectToRoute(AdvieskeuzeRoutes.Blogs());
      BlogTrafficData.Log(RequestUtil, model.ID, false, true);
      return RedirectToRoute(AdvieskeuzeRoutes.ShowLocatie(model.Locatie));
    }
    // MHTODO : Clickevent ook loggen!

  }
}
