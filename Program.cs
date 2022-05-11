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
var webService = new WebSiteService(dateAttributes);
var emailClient = new EmailClient();
var cacheService = new CacheService();
var emailBody = new StringBuilder();

//var pageContent = webService.GetPageContent("https://www.sciencealert.com/look-up-the-eta-aquariid-meteor-shower-is-set-to-light-up-the-skies");

//var p = 10;

//emailBody.AppendLine("<h1>Web Scraper Results</h1>");

//emailClient.SendEmail(settings.EmailSettings, new Email() { Subject = "Scrape Results", Body = emailBody.ToString() });

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
  // Get new pages
  // Filter by keywords
  // ForEach relevant page > add to email body
}

// Send email

// Close WebDriver
webService.DisposeService();

Console.WriteLine("Done");