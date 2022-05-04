using System.Text.Json;
using WebScraper.Models;

namespace WebScraper;

public class CacheService
{
  private const string cacheDirectory = "c:\\WebScraperCache";

  public CacheService()
  {
    InitializeCache();
  }
  public void InitializeCache()
  {
    Directory.CreateDirectory(cacheDirectory);
  }

  public string CacheFileNameFromUrl(string url)
  {
    var uri = new Uri(url);
    return uri.Authority;
  }

  public async Task<SiteCache> GetSiteCache(string url)
  {
    var siteCache = new SiteCache();
    try
    {
      var filePath = Path.Combine(cacheDirectory, $"{CacheFileNameFromUrl(url)}.json");
      using var streamReader = new StreamReader(filePath);
      var json = await streamReader.ReadToEndAsync();
      var cache = JsonSerializer.Deserialize<SiteCache>(json);
      if (cache != null)
      {
        return cache;
      }
    }
    catch (FileNotFoundException e)
    {
      Console.WriteLine($"The file for {url} could not be read:");
      Console.WriteLine(e.Message);
    }

    return siteCache;
  }

  public async Task SetSiteCache(string url, SiteCache siteCache)
  {
    try
    {
      var filePath = Path.Combine(cacheDirectory, $"{CacheFileNameFromUrl(url)}.json");
      await using var streamWriter = new StreamWriter(filePath, false);
      var json = JsonSerializer.Serialize(siteCache);
      await streamWriter.WriteAsync(json);
    }
    catch (FileNotFoundException e)
    {
      Console.WriteLine($"The file for {url} could not be written to:");
      Console.WriteLine(e.Message);
      throw;
    }
  }
}