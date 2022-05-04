using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebScraper.Models;

namespace WebScraper;

public class Scraper
{
  private WebDriver _chromeDriver;
  public Scraper()
  {
    ChromeOptions options = new ChromeOptions();
    options.AddArgument("--headless");
    options.AddArgument("--log-level=2");
    options.AddArgument("--silent");
    options.AddArgument("--ignore-certificate-errors");
    options.AddArgument("--ignore-ssl-errors");
    _chromeDriver = new ChromeDriver(options);
  }

  public void DisposeWebDriver()
  {
    _chromeDriver.Dispose();
    _chromeDriver.Quit();
  }

  public List<string> GetLinksFromPage(string pageUrl)
  {
    var pageLinkUrls = new List<string>();
    try
    {
      _chromeDriver.Navigate().GoToUrl(pageUrl);
      var aTags = _chromeDriver.FindElements(By.TagName("a"));
      foreach (var element in aTags)
      {
        pageLinkUrls.Add(element.GetAttribute("href"));
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: GetLinksFromPage at : " + pageUrl + "\n" + ex.Message);
    }
    return pageLinkUrls.Distinct().ToList();
  }

  public async Task<List<string>> GetSiteUrls(string siteUrl, SiteCache? siteCache = null, List<string>? blackList = null)
  {
    // Site host to limit to links in current site
    var siteHost = new Uri(siteUrl).Host;
    // Unique links only
    var uniqueSiteUrls = new HashSet<string>();
    // Merge with cache if available
    uniqueSiteUrls.UnionWith(siteCache?.Urls ?? new List<string>());
    // Crawl site links recursively
    GetPageLinks(siteUrl);

    return uniqueSiteUrls.ToList();

    // Local Recursive Function
    async void GetPageLinks(string pageUrl)
    {
      // Scrape the current page
      var pageLinks = await Task.FromResult(GetLinksFromPage(pageUrl));
      if (!pageLinks.Any())
      {
        return;
      }
      // Filter
      var filteredLinks = pageLinks.Where(x => LinkIsValidUrl(x)
                                               && blackList != null && !blackList.Any(subUrl => x.Contains(subUrl))
                                               && x.Contains(siteHost)
                                               && !uniqueSiteUrls.Contains(x)).ToList();

      // Merge
      uniqueSiteUrls.UnionWith(filteredLinks);
      // Recurse
      foreach (var link in filteredLinks)
      {
        GetPageLinks(link);
      }
    }
  }

  public bool LinkIsValidUrl(string link)
  {
    // Check for valid link
    if (link == null)
    {
      return false;
    }
    Uri uriResult;
    var result = Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    return result;
  }

}