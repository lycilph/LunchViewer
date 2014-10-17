using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MailWorkerRole.CSES.SmtpServer;
using MimeKit;

namespace MailWorkerRole.CSES.Implementation
{
    public class MessageSpool : IMessageSpool
    {
        public bool SpoolMessage(SMTPMessage message)
        {
            Trace.TraceInformation("Spooling message from " + message.FromAddress);
            Trace.TraceInformation(message.Data);

            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(message.Data)))
            {
                var mime_message = MimeMessage.Load(s);
                Trace.TraceInformation("Part count: " + mime_message.BodyParts.Count());

                foreach (var part in mime_message.BodyParts)
                {
                    Trace.TraceInformation("{0} - {1} - {2}", part.ContentType.MimeType, part.ContentType.MediaType, part.ContentType.MediaSubtype);
                    if (part.ContentType.MimeType == "text/html")
                        Trace.TraceInformation("Text: " + ((TextPart) part).Text.Trim());
                    else
                        Trace.TraceInformation("Wrong message type");
                }
            }

            return true;
        }
    }
}
