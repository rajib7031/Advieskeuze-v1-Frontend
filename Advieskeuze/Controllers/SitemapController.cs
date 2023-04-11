using AdvieskeuzeCode.Data;
using AdvieskeuzeCode.DataModel;
using AdvieskeuzeCode.DataModel.Advieskeuze;
using MvcSiteMap.Core;
using SharedCode;
using SimpleMvcSitemap;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Advieskeuze.Controllers {
  /// <summary>
  /// Rendert een aantal XML sitemaps voor zoekmachines op basis van het http://www.sitemaps.org/ protocol.
  /// </summary>
  /// De sitemap bestaat uit 2 delen. De index gebruikt de Advieskeuze.sitemap voor de 'losse' pagina's. De
  /// kantoren, beoordelingen en kennisbank zijn een simpele lijst van items via een sitemap-lijst-generator.
  /// 
  /// Performance is hier belangrijk. Dit gedeelte van de website is namelijk niet zo relevant voor de 
  /// gebruikerservaring maar wel voor de zoekmachines. Periodieke zoekmachine scrape acties mogen
  /// geen invloed hebben op de ervaring van de consument. 
  /// <remarks>
  /// De lijsten zijn gevoelig voor performance veranderingen. Selecteer alleen dat wat snel opgehaald
  /// kan worden. (Bijv. beoordeling aanpas datum zou je kunnen achterhalen maar dat gaat de performance
  /// volledig killen).
  /// De lijsten zitten een uur in cache omdat zoekmachines het niet zo actueel hoeven te hebben.
  /// </remarks>
  public class SitemapController : Controller {
    [OutputCache(CacheProfile = "DynamicPage")]
    public ActionResult Index() {
      var nodesToRender = new SiteMapNodeCollection();
      nodesToRender.AddRange(SiteMap.RootNode.ChildNodes);
      var nodes = (nodesToRender.OfType<MvcSiteMapNode>().Select(n => new SitemapNode(n.Url.Split('?')[0].Replace("~/", GewrapteSettings.HostUrl)))).ToList();
      // Remove a few incorrectly generated nodes
      var node = nodes.FirstOrDefault(n => n.Url.Contains("nlbeoordeling"));
      if (node != null)
        nodes.Remove(node);
      node = nodes.FirstOrDefault(n => n.Url.Contains("zoekmodule"));
      if (node != null)
        nodes.Remove(node);
      return new SitemapProvider().CreateSitemap(HttpContext, nodes);
    }

    [OutputCache(CacheProfile = "DynamicPage", VaryByParam = "id")]
    public ActionResult Kantoren(int? id) {
      var dataSource = LocatieData.GetForSitemap();
      var configuration = new LocatieSitemapConfiguration(id);
      return new SitemapProvider().CreateSitemap(HttpContext, dataSource, configuration);
    }

    [OutputCache(CacheProfile = "DynamicPage", VaryByParam = "id")]
    public ActionResult Beoordelingen(int? id) {
      var dataSource = BeoordelingData.GetForSitemap();
      var configuration = new BeoordelingSitemapConfiguration(id);
      return new SitemapProvider().CreateSitemap(HttpContext, dataSource, configuration);
    }

    [OutputCache(CacheProfile = "DynamicPage", VaryByParam = "id")]
    public ActionResult Kennisbank(int? id) {
      var dataSource = KennisbankData.GetForSitemap();
      var configuration = new KennisbankSitemapConfiguration(id);
      return new SitemapProvider().CreateSitemap(HttpContext, dataSource, configuration);
    }

    [OutputCache(CacheProfile = "DynamicPage", VaryByParam = "id")]
    public ActionResult Blogs(int? id) {
      var dataSource = BlogData.GetForSitemap();
      var configuration = new BlogSitemapConfiguration(id);
      return new SitemapProvider().CreateSitemap(HttpContext, dataSource, configuration);
    }

  }

  public class LocatieSitemapConfiguration : ISitemapConfiguration<LocatieSitemapModel> {
    public int? CurrentPage { get; private set; }
    public int Size { get; private set; }

    public LocatieSitemapConfiguration(int? currentPage) {
      Size = 10000;
      CurrentPage = currentPage;
    }

    public string CreateSitemapUrl(int currentPage) {
      return $"{GewrapteSettings.HostUrl}/sitemap/kantoren/{currentPage}";
    }

    public SitemapNode CreateNode(LocatieSitemapModel source) {
      return new SitemapNode(Locatie.AdvieskeuzePaginaUrlCreator(source.Postcode, source.Slug)) {
        ChangeFrequency = ChangeFrequency.Monthly,
        Priority = 1.0M
      };
    }
  }

  public class BeoordelingSitemapConfiguration : ISitemapConfiguration<BeoordelingSitemapModel> {
    public int? CurrentPage { get; private set; }
    public int Size { get; private set; }

    public BeoordelingSitemapConfiguration(int? currentPage) {
      Size = 10000;
      CurrentPage = currentPage;
    }

    public string CreateSitemapUrl(int currentPage) {
      return $"{GewrapteSettings.HostUrl}/sitemap/beoordelingen/{currentPage}";
    }

    public SitemapNode CreateNode(BeoordelingSitemapModel source) {
      return new SitemapNode(Beoordeling.GetDetailsUrlAbsolute(source.LocatiePostcode, source.LocatieSlug, source.BeoordelingSlug)) {
        ChangeFrequency = ChangeFrequency.Never,
        LastModificationDate = DateTime.Parse(DateTimeExtensions.ConvertDateToW3CTime(source.LastModificationDate)),
        Priority = 0.7M
      };
    }
  }

  public class KennisbankSitemapConfiguration : ISitemapConfiguration<ArtikelSitemapModel> {
    public int? CurrentPage { get; private set; }
    public int Size { get; private set; }

    public KennisbankSitemapConfiguration(int? currentPage) {
      Size = 10000;
      CurrentPage = currentPage;
    }

    public string CreateSitemapUrl(int currentPage) {
      return $"{GewrapteSettings.HostUrl}/sitemap/kennisbank/{currentPage}";
    }

    public SitemapNode CreateNode(ArtikelSitemapModel source) {
      return new SitemapNode(Artikel.GetDetailsUrlAbsolute(source.Slug)) {
        ChangeFrequency = ChangeFrequency.Never,
        LastModificationDate = DateTime.Parse(DateTimeExtensions.ConvertDateToW3CTime(source.LastModificationDate)),
        Priority = 0.5M
      };
    }
  }

  public class BlogSitemapConfiguration : ISitemapConfiguration<BlogSitemapModel> {
    public int? CurrentPage { get; private set; }
    public int Size { get; private set; }

    public BlogSitemapConfiguration(int? currentPage) {
      Size = 10000;
      CurrentPage = currentPage;
    }

    public string CreateSitemapUrl(int currentPage) {
      return $"{GewrapteSettings.HostUrl}/sitemap/blogs/{currentPage}";
    }

    public SitemapNode CreateNode(BlogSitemapModel source) {
      return new SitemapNode(Blog.GetDetailsUrlAbsolute(source.Slug)) {
        ChangeFrequency = ChangeFrequency.Never,
        LastModificationDate = DateTime.Parse(DateTimeExtensions.ConvertDateToW3CTime(source.LastModificationDate)),
        Priority = 0.8M
      };
    }
  }
}