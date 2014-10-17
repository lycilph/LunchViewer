using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MailWorkerRole.CSES.SmtpServer;
using MailWorkerRole.Utils;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using MimeKit;
using RestSharp;

namespace MailWorkerRole.CSES.Implementation
{
    public class MessageSpool : IMessageSpool
    {
        public bool SpoolMessage(SMTPMessage message)
        {
            Trace.TraceInformation("Spooling message from " + message.FromAddress);

            var domain = RoleEnvironment.GetConfigurationSettingValue("MobileServiceDomainName");

            var storage_account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageAccount"));
            var blob_client = storage_account.CreateCloudBlobClient();
            var container = blob_client.GetContainerReference("email");
            container.CreateIfNotExists();

            // Forward message to mobile service
            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(message.Data)))
            {
                var mime_message = MimeMessage.Load(s);
                Trace.TraceInformation("Part count: " + mime_message.BodyParts.Count());

                var part = mime_message.BodyParts.SingleOrDefault(p => p.ContentType.MimeType == "text/html");
                if (part != null)
                {
                    var text_part = part as TextPart;
                    if (text_part != null)
                    {
                        var text = text_part.Text.Trim();

                        Trace.TraceInformation("Saving mail in blob storage");

                        // Save message in blob storage
                        var id = text.ComputeMD5Hash();
                        var block = container.GetBlockBlobReference(id);
                        block.UploadText(message.Data);

                        Trace.TraceInformation("Forwarding content to {0}", domain);

                        var client = new RestClient(domain);
                        var request = new RestRequest("api/Mail", Method.POST);
                        request.AddParameter("text/xml", text, ParameterType.RequestBody);

                        var response = client.Execute(request);
                        if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                            Trace.TraceError("Mail api error: " + response.ErrorMessage);
                        else
                            Trace.TraceInformation("Mail api response ({0}): {1}", response.StatusCode, response.Content);
                    }
                    else
                        Trace.TraceWarning("Part could not be cast to TextPart");
                }
                else
                    Trace.TraceWarning("Unknown content found");
            }

            return true;
        }
    }
}
