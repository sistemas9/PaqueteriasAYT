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
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            //client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("notificacionatp@avanceytec.com.mx", "ub*M[2y[Yv");
            client.EnableSsl = true;

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
