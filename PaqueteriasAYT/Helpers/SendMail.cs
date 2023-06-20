using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PaqueteriasAYT.Helpers
{
    public class SendMail
    {
        public static void Send(string from, string to, string body, string subject) {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.office365.com";
            client.Port = 587;
            //client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("notificaciones@avanceytec.com.mx", "8hn?X#~J?rvOI.+#=:E1S1}2C3xi6h)K");
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from);
            mailMessage.To.Add(to);
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;
            client.Send(mailMessage);
        }
    }
}
