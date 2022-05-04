namespace WebScraper.Models;

public class EmailSettings
{
  public string SenderUserName { get; set; }
  public string SenderEmail { get; set; }
  public string SenderPassword { get; set; }
  public string ReceiverUserName { get; set; }
  public string ReceiverEmail { get; set; }
  public string Host { get; set; }
  public int Port { get; set; }
}