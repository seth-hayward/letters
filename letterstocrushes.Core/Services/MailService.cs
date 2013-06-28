using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Services
{
    public class MailService
    {

        private String l_mail_password;

        public MailService(String mail_password)
        {
            l_mail_password = mail_password;
        }

        public void SendPasswordLink(string message_body, string email_address)
        {
            MailMessage Message = new MailMessage();
            SmtpClient Smtp = new SmtpClient();
            System.Net.NetworkCredential SmtpUser = new System.Net.NetworkCredential("noreply@letterstocrushes.com", l_mail_password);

            Message.From = new MailAddress("noreply@letterstocrushes.com");
            Message.To.Add(new MailAddress(email_address));
            Message.Bcc.Add(new MailAddress("seth.hayward@gmail.com"));
            Message.IsBodyHtml = false;
            Message.Subject = "letters to crushes password request";
            Message.Body = message_body;
            Message.Priority = MailPriority.Normal;
            Smtp.EnableSsl = false;

            Smtp.Credentials = SmtpUser;
            Smtp.Host = "mail.letterstocrushes.com";
            Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            Smtp.Port = 26;

            Smtp.Send(Message);
        }

        public void SendContact(string message_body, string email_address)
        {
            MailMessage Message = new MailMessage();
            SmtpClient Smtp = new SmtpClient();
            System.Net.NetworkCredential SmtpUser = new System.Net.NetworkCredential("noreply@letterstocrushes.com", l_mail_password);

            message_body = "<html><head></head><body>" + message_body + "</body></html>";

            Message.From = new MailAddress("noreply@letterstocrushes.com");
            Message.To.Add(new MailAddress("seth.hayward@gmail.com"));
            Message.IsBodyHtml = true;
            Message.Subject = "feedback: " + email_address;
            Message.Body = message_body;
            Message.Priority = MailPriority.Normal;
            Smtp.EnableSsl = false;

            Smtp.Credentials = SmtpUser;
            Smtp.Host = "198.57.199.92";
            Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            Smtp.Port = 26;

            List<string> ignore_phrases = new List<string>();
            ignore_phrases.Add("url=http:");

            Smtp.Send(Message);
        }

        public void SendCommentNotification(string message_body, string email_address)
        {

            MailMessage Message = new MailMessage();
            SmtpClient Smtp = new SmtpClient();
            System.Net.NetworkCredential SmtpUser = new System.Net.NetworkCredential("noreply@letterstocrushes.com", l_mail_password);

            message_body = "<html><head></head><body>" + message_body + "</body></html>";

            MailAddress user_to_notify;

            try
            {
                user_to_notify = new MailAddress(email_address);
            }
            catch (Exception ex)
            {
                // whatever, ditch this notification
                return;
            }

            Message.From = new MailAddress("noreply@letterstocrushes.com");
            Message.To.Add(user_to_notify);
            //Message.Bcc.Add(new MailAddress("seth.hayward@gmail.com"));
            Message.IsBodyHtml = true;
            Message.Subject = "New letters to crushes comment";
            Message.Body = message_body;
            Message.Priority = MailPriority.Normal;
            Smtp.EnableSsl = false;

            Smtp.Credentials = SmtpUser;
            Smtp.Host = "198.57.199.92";
            Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            Smtp.Port = 26;

            Smtp.Send(Message);

        }


    }
}
