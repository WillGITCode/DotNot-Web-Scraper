using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebScraper.Models;
using WebScraper.Utilities;

namespace WebScraper;

public class WebSiteService
{
  private readonly WebDriver _chromeDriver;
  private readonly string[] DATE_ATTRIBUTES;
  private readonly Regex _dateRegex = new(@"\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])");
  public WebSiteService(string[]? dateAttributes)
  {
    var options = new ChromeOptions();
    options.AddArgument("--headless");
    options.AddArgument("--log-level=2");
    options.AddArgument("--silent");
    options.AddArgument("--ignore-certificate-errors");
    options.AddArgument("--ignore-ssl-errors");
    _chromeDriver = new ChromeDriver(options);
    if (dateAttributes != null)
    {
      DATE_ATTRIBUTES = dateAttributes;
    }
  }

  public void DisposeService()
  {
    _chromeDriver.Dispose();
    _chromeDriver.Quit();
  }

  public DateTime TryGetPagePublishDate(string pageUrl)
  {
    //_chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);
    _chromeDriver.Navigate().GoToUrl("https://phys.org/news/2022-05-sleuthing-3d-quantum-liquid.html");
    // Brute force method to try FIND the publish date
    foreach (var dateAttribute in DATE_ATTRIBUTES)
    {
      var potentialDateTag = _chromeDriver.FindElements(By.ClassName(dateAttribute));
      if (potentialDateTag.Any())
      {
        // Brute force method to try EXTRACT the publish date
        var dateDotValue = potentialDateTag[0].GetAttribute("value");
        if (DateTime.TryParse(dateDotValue, out var firstDateAttempt))
        {
          return firstDateAttempt;
        }
        var dateByAttribute = potentialDateTag[0].GetAttribute("datetime");
        if (DateTime.TryParse(dateByAttribute, out var secondDateAttempt))
        {
          return secondDateAttempt;
        }
      }
    }
    // Failed to find a tag with a date value/attribute, so try regex on source :)
    var firstRegexDateValue = _dateRegex.Match(_chromeDriver.PageSource);
    if (firstRegexDateValue.Success && DateTime.TryParse(firstRegexDateValue.Value, out var thirdDateAttempt))
    {
      return thirdDateAttempt;
    }
    // Failed, return current time as placeholder
    return DateTime.Now.ToUniversalTime();
  }

  public string GetSiteSourcePage(string url)
  {
    _chromeDriver.Url = url;
    return _chromeDriver.PageSource;
  }

  public List<string> GetLinksPresentAtPage(string pageUrl)
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
      Console.WriteLine("Error: GetLinksPresentAtPage at : " + pageUrl + "\n" + ex.Message);
    }
    return pageLinkUrls.Distinct().ToList();
  }

  public async Task<Dictionary<string, DateTime>> GenerateSiteMapRecursively(string siteUrl, SiteCache? siteCache = null, List<string>? blackList = null)
  {
    // Site host to limit to links in current site
    var siteHost = new Uri(siteUrl).Host;
    // Unique pages only
    var uniqueSiteUrls = DataHelper.MergeSiteMapInPlace(new Dictionary<string, DateTime>(), siteCache?.PublishedPages);
    // Crawl site links recursively
    await Task.Run(() => GetPageLinks(siteUrl));

    return uniqueSiteUrls;

    // Local Recursive Function
    async void GetPageLinks(string pageUrl)
    {
      // Scrape all links at current page
      var pageLinks = await Task.FromResult(GetLinksPresentAtPage(pageUrl));
      if (!pageLinks.Any())
      {
        return;
      }
      // Filter
      var filteredLinks = pageLinks.Where(x => LinkIsValidUrl(x)
                                               && blackList != null && !blackList.Any(subUrl => x.Contains(subUrl))
                                               && x.Contains(siteHost)
                                               && !uniqueSiteUrls.Any(u => u.Key.Equals(x)));
      // Init subset of site map
      var subMap = new Dictionary<string, DateTime>();
      foreach (var page in filteredLinks)
      {
        subMap.Add(page, TryGetPagePublishDate(page));
      }

      // In place Merge
      uniqueSiteUrls = DataHelper.MergeSiteMapInPlace(uniqueSiteUrls, subMap);
      // Recurse
      foreach (var pageElement in subMap)
      {
        GetPageLinks(pageElement.Key);
      }
    }
  }

  public bool LinkIsValidUrl(string? link)
  {
    // Check for valid link
    if (link == null | link == "")
    {
      return false;
    }
    Uri uriResult;
    var result = Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                  && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    return result;
  }

}