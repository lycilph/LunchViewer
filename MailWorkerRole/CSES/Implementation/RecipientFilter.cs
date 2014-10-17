using System.Diagnostics;
using MailWorkerRole.CSES.Common;
using MailWorkerRole.CSES.SmtpServer;

namespace MailWorkerRole.CSES.Implementation
{
    public class RecipientFilter : IRecipientFilter
    {
        public bool AcceptRecipient(SMTPContext context, EmailAddress recipient)
        {
            Trace.TraceInformation("Recipient: " + recipient.Address);
            return true;
        }
    }
}
