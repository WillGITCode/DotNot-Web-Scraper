using System.Text;
using Microsoft.Extensions.Configuration;
using WebScraper;
using WebScraper.Models;

// Setup the configuration for the application
IConfiguration Configuration = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
  .AddJsonFile("appsettings.dev.json", true, true)
  .AddEnvironmentVariables()
  .AddCommandLine(args)
  .Build();

// Member variables
var settings = Configuration.Get<Settings>();
var dateAttributes = settings.Constants?.DATE_ATTRIBUTES ?? new string[] { };
var keywordAttributes = settings.Constants?.KEYWORD_ATTRIBUTES ?? new string[] { };
var webService = new WebSiteService(dateAttributes, keywordAttributes);
var emailClient = new EmailClient();
var cacheService = new CacheService();
var emailBody = new StringBuilder();

//var pageContent = webService.GetPageContent("https://www.sciencealert.com/look-up-the-eta-aquariid-meteor-shower-is-set-to-light-up-the-skies");

//var p = 10;

//emailBody.AppendLine("<h1>Web Scraper Results</h1>");

//emailClient.SendEmail(settings.EmailSettings, new Email() { Subject = "Scrape Results", Body = emailBody.ToString() });

// Create/Update site cache
foreach (var site in settings.Sites)
{
  // Get site cache
  var siteCache = await cacheService.GetSiteCache(site);
  // Create complete site map
  var siteMap = await webService.GenerateSiteMapRecursively(site, siteCache, settings.BlackList);
  // Update site cache
  await cacheService.SetSiteCache(site, new SiteCache()
  {
    PublishedPages = siteMap
  });
}

// TryGet recent pages
foreach (var site in settings.Sites)
{
  // Get site cache
  var siteCache = await cacheService.GetSiteCache(site);
  // Sort site urls by date
  var sortedPageUrls = siteCache?.PublishedPages?.OrderByDescending(x => x.Value).ToList();
  // Get new pages according to settings
  var publicationTime = DateTime.Now.AddDays(-settings.PublishedSinceHowManyDays).ToUniversalTime();
  var recentPageUrls = sortedPageUrls?.FindAll(x => x.Value >= publicationTime);
  foreach (var page in recentPageUrls)
  {
    // Filter by keywords
    var pageKeywords = webService.TryGetPageKeywords(page.Key);
    var key = "asdf";

  }
  // ForEach relevant page > add to email body
}



// Send email

// Close WebDriver
webService.DisposeService();

Console.WriteLine("Done");