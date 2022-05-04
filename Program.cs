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
var scraper = new Scraper();
var emailClient = new EmailClient();
var cacheService = new CacheService();
var emailBody = new StringBuilder();
emailBody.AppendLine("<h1>Web Scraper Results</h1>");

emailClient.SendEmail(settings.EmailSettings, new Email() { Subject = "Scrape Results", Body = emailBody.ToString() });

//foreach (var site in settings.Sites)
//{
//  // Get site cache
//  var siteCache = await cacheService.GetSiteCache(site);
//  // Create complete site map
//  var siteUrls = await scraper.GetSiteUrls(site, siteCache, settings.BlackList);
//  // Update site cache
//  await cacheService.SetSiteCache(site, new SiteCache()
//  {
//    Urls = siteUrls
//  });
//  // Get new pages
//  // Filter by keywords
//  // ForEach relevant page > add to email body
//}

// Send email

// Close WebDriver
scraper.DisposeWebDriver();

Console.WriteLine("Done");