using AdvieskeuzeCode.DataModel;
using System.Collections.Generic;

namespace Advieskeuze.Models {
  public class BlogIndexModel {
    public List<Blog> Blogs { get; set; }
    public string Zoektermen { get; set; }
  }
  public class BlogcategorieModel {
    public Blogcategorie Blogcategorie { get; set; }
    public List<Blog> Blogs { get; set; }
    public string Zoektermen { get; set; }
    public BlogcategorieModel() {
      this.Blogs = new List<Blog>();
    }
  }
}