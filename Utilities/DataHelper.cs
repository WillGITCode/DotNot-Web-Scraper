namespace WebScraper.Utilities;

public static class DataHelper
{
  public static Dictionary<string, DateTime> MergeSiteMapInPlace(Dictionary<string, DateTime>? siteMap, Dictionary<string, DateTime>? subMap)
  {
    if (siteMap == null)
    {
      throw new ArgumentNullException("Can't merge into a null dictionary");
    }
    if (subMap == null)
    {
      return siteMap;
    }

    foreach (var kvp in subMap)
    {
      if (!siteMap.ContainsKey(kvp.Key))
      {
        siteMap.Add(kvp.Key, kvp.Value);
      }
    }

    return siteMap;
  }
}