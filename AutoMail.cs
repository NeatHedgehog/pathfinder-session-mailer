using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;

namespace AutoMailerApp
{
    public class AutoMail
    {
        public static void SendMail(string _name, string _email, string _sessionTime, Guid _rsvpGuid)
        {
            string[] rawMessage = File.ReadAllLines("/home/dockeruser/AutoMailerSystemFiles/MasterEmailTemplate.txt");
            string messageBody = "";

            for (int i = 0; i < rawMessage.Length; i++)
            {
                messageBody += rawMessage[i] + "<br />";
            }

            messageBody = messageBody.Replace("[TIME]", _sessionTime).Replace("[RSVP]", "<b><a href=\"http://hedgehogden.no-ip.org:36666/RSVP/RSVPIndex?validationGuid=" + _rsvpGuid.ToString() + "\">RSVP</a></b>");

            BodyBuilder bodyBuilder = new BodyBuilder() { HtmlBody = messageBody };
            MimeMessage message = new MimeMessage();

            message.From.Add(new MailboxAddress("Carl","neathedgehog@charter.net"));
            message.To.Add(new MailboxAddress(_name,_email));
            message.Subject = "New session at " + _sessionTime;
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.charter.net", 25, false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
