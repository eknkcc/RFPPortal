using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Utility
{
    public class EmailHelper
    {
        /// <summary>
        ///  Sends email using SMTP
        /// </summary>
        /// <param name="Subject">Email subject</param>
        /// <param name="Content">Email content</param>
        /// <param name="To">To list</param>
        /// <param name="Cc">Cc list</param>
        /// <param name="Bcc">Bcc list</param>
        /// <returns></returns>
        public static string SendEmail(string Subject, string Content, List<string> To, List<string> Cc, List<string> Bcc)
        {
            try
            {
                List<MailAddress> lst = new List<MailAddress>();

                System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage();
                m.From = new System.Net.Mail.MailAddress(Program._settings.EmailAddress, Program._settings.EmailDisplayName);

                foreach (var item in To)
                {
                    m.To.Add(new MailAddress(item));
                }
                foreach (var item in Cc)
                {
                    m.CC.Add(new MailAddress(item));
                }
                foreach (var item in Bcc)
                {
                    m.Bcc.Add(new MailAddress(item));
                }

                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(Program._settings.EmailHost);

                m.Subject = Subject;
                m.Body = WrapToMailTemplate(Content);
                m.IsBodyHtml = true;

                smtp.Port = Convert.ToInt32(Program._settings.EmailPort);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                if (Convert.ToBoolean(Program._settings.EmailSSL))
                    smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(Program._settings.EmailAddress, Program._settings.EmailPassword);
                smtp.Send(m);

                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        ///  Wraps email content into html template
        /// </summary>
        /// <param name="content">Email content</param>
        /// <returns></returns>
        public static string WrapToMailTemplate(string content)
        {
            string all =
            "<style>.btnspecial{text-decoration:none;padding:10px 20px;border-radius:5px;background:#334d80;color:#f3f3f3;}</style>" +

            "<div style=\"width:100%;height:100px;background:#353847 !important;border-bottom: 5px solid #334d80;\">" +
                "<center>" +
                    "<h3 style=\"color:#ffffff;margin-top:20px\">RFP PORTAL</h3>" +
                "</center>" +
            "</div>" +

            "<div style=\"width:100%;min-height:300px;background:#f3f3f3 !important;padding:50px;color:#25262f;font-family:Arial\">" +
                         content +
            "</div>" +

            "<div style=\"width:100%;height:100px;background:#353847 !important;border-top: 5px solid #334d80;\">" +

            "</div>";

            return all;
        }
    }
}
