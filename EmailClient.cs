using System.Net;
using System.Net.Mail;
using WebScraper.Models;

namespace WebScraper;

public class EmailClient
{
  public void SendEmail(EmailSettings settings, Email email)
  {
    try
    {
      var fromAddress = new MailAddress(settings.SenderEmail, settings.SenderUserName);
      var toAddress = new MailAddress(settings.ReceiverEmail, settings.ReceiverUserName);

      var smtp = new SmtpClient
      {
        Host = settings.Host,
        Port = settings.Port,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddress.Address, settings.SenderPassword)
      };
      using var message = new MailMessage(fromAddress, toAddress)
      {
        Subject = email.Subject,
        Body = email.Body
      };
      smtp.Send(message);
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }
  }
}