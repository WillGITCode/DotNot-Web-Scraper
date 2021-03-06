namespace WebScraper.Models;

public class Settings
{
  public EmailSettings EmailSettings { get; set; }
  public List<string> Sites { get; set; }
  public List<string> BlackList { get; set; }
  public List<string> Keywords { get; set; }
  public int PublishedSinceHowManyDays { get; set; }
  public ApplicationConstants? Constants { get; set; }
}