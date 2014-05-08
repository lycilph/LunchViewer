using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.ServiceBus.Notifications;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MimeKit;
using ModelLibrary;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EmailProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        private CloudTable menu_table;
        private CloudTable item_table;
        private CloudTable error_table;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("EmailProcessor entry point called");

            while (true)
            {
                Trace.TraceInformation("Checking mail");
                GetOrCreateTables();

                try
                {
                    using (var client = new ImapClient())
                    {
                        var credentials = new NetworkCredential("firstlast_lunchviewer@outlook.com", "k4hfjf93JK3");
                        var uri = new Uri("imaps://imap-mail.outlook.com");

                        using (var cancel = new CancellationTokenSource())
                        {
                            client.Connect(uri, cancel.Token);
                            // Note: since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
                            client.AuthenticationMechanisms.Remove("XOAUTH");
                            client.Authenticate(credentials, cancel.Token);

                            // The Inbox folder is always available on all IMAP servers...
                            var inbox = client.Inbox;
                            inbox.Open(FolderAccess.ReadWrite, cancel.Token);

                            Trace.TraceInformation("Total messages: {0}", inbox.Count);
                            Trace.TraceInformation("Recent messages: {0}", inbox.Recent);

                            // Get unread messages
                            var unread_mails = inbox.Fetch(0, -1, MessageSummaryItems.Flags).Where(s => s.Flags.HasValue && !s.Flags.Value.HasFlag(MessageFlags.Seen)).ToList();
                            Trace.TraceInformation("Unread messages: {0}", unread_mails.Count());

                            if (unread_mails.Any())
                            {
                                // Get message indices and set messages read flag
                                var indices = unread_mails.Select(s => s.Index).ToArray();
                                inbox.AddFlags(indices, MessageFlags.Seen, true, cancel.Token);

                                foreach (var summary in unread_mails)
                                {
                                    var message = inbox.GetMessage(summary.Index);

                                    if (IsValidMenuMail(message))
                                    {
                                        Trace.TraceInformation("[Mail] {0:D2}: {1}", summary.Index, message.Subject);

                                        Menu menu;
                                        if (TryParseMessage(message, out menu))
                                        {
                                            SaveMenu(menu);
                                            SendNotification(menu).Wait();
                                        }
                                    }
                                    else
                                    {
                                        var error_message = string.Format("Not a valid menu mail (subject {0})",  message.Subject);

                                        AddError("Invalid Mail", error_message);
                                        Trace.TraceInformation(error_message);
                                    }
                                }
                            }

                            client.Disconnect(true, cancel.Token);
                        }
                    }
                }
                catch (Exception e)
                {
                    var message = e.GetType().ToString() + " " + e.Message;
                    AddError("General Error", message);
                }

                var next_execution_time = DateTime.Now.AddHours(1);
                var current_time = DateTime.Now;

                Trace.TraceInformation("Waiting until {0}", next_execution_time.ToShortTimeString());
                while (current_time < next_execution_time)
                {

                    Thread.Sleep(1000);
                    current_time = DateTime.Now;
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private void GetOrCreateTables()
        {
            var storage_account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var table_client = storage_account.CreateCloudTableClient();

            menu_table = table_client.GetTableReference("Menus");
            menu_table.CreateIfNotExists();

            item_table = table_client.GetTableReference("Items");
            item_table.CreateIfNotExists();

            error_table = table_client.GetTableReference("Errors");
            error_table.CreateIfNotExists();
        }

        private static async Task SendNotification(Menu menu)
        {
            var hub_connection_string = RoleEnvironment.GetConfigurationSettingValue("NotificationHubConnectionString");
            var hub_name = RoleEnvironment.GetConfigurationSettingValue("NotificationHubName");
            var hub = NotificationHubClient.CreateClientFromConnectionString(hub_connection_string, hub_name, true);

            var registrations = await hub.GetAllRegistrationsAsync(Int32.MaxValue);
            Trace.TraceInformation("Found {0} registrations", registrations.Count());

            if (registrations.Any())
            {
                var message = string.Format("Processed new menu ({0}, {1})", menu.MenuEntity.Year, menu.MenuEntity.Week);
                var notification = new WindowsNotification(message);
                notification.Headers.Add("X-WNS-Type", "wns/raw");

                var outcome = await hub.SendNotificationAsync(notification);
                Trace.TraceInformation("Notification outcome {0}", outcome.State);
            }
        }

        private void AddError(string type, string message)
        {
            var error = new ErrorEntity(type, message);
            var op = TableOperation.Insert(error);
            error_table.Execute(op);
        }

        private static bool TryParseMessage(MimeMessage message, out Menu menu)
        {
            menu = null;

            foreach (var part in message.BodyParts)
            {
                Trace.TraceInformation(" - {0} {1}", part.ContentType.MediaType, part.ContentType.MediaSubtype);
                if (part.ContentType.MediaType == "text" && part.ContentType.MediaSubtype == "html")
                {
                    var text_part = part as TextPart;
                    if (text_part == null)
                        return false;

                    var doc = new HtmlDocument();
                    doc.LoadHtml(text_part.Text);

                    // Find current year and week for the message
                    var week = -1;
                    var matches = Regex.Matches(doc.DocumentNode.InnerText, @"(Menuer i denne uge) (\d*)", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                        week = int.Parse(matches[0].Groups[2].ToString());
                    var year = DateTime.Now.Year;

                    menu = new Menu(year, week);

                    // Find the individual items (or days)
                    var items = doc.DocumentNode.Descendants().Where(n => n.Name == "td" &&
                                                                  n.Attributes.Contains("class") &&
                                                                  n.Attributes["class"].Value == "productItem");

                    // Parse item
                    foreach (var item in items)
                    {
                        var rows = item.Descendants("tr").Select(n => n.InnerText.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                        var date = DateTime.ParseExact(rows[0], "dddd 'd.' d. MMMM", CultureInfo.CreateSpecificCulture("da-DK"));
                        var text = string.Format("{0} {1}", HtmlEntity.DeEntitize(rows[3]).Trim(), HtmlEntity.DeEntitize(rows[4]).Trim());

                        Trace.TraceInformation("-----------------------------------------------------------------------");
                        Trace.TraceInformation(date.ToString("dddd 'd.' d. MMMM", CultureInfo.CreateSpecificCulture("da-DK")));
                        Trace.TraceInformation(text);

                        menu.Add(date, text);
                    }

                    return true;
                }

                Trace.TraceInformation(" --- Wrong content type");
            }

            return false;
        }

        private static bool IsValidMenuMail(MimeMessage message)
        {
            if (message.From.Count != 1)
                return false;

            var mailbox = message.From[0] as MailboxAddress;
            if (mailbox == null)
                return false;

            return string.Compare(mailbox.Address, "oticon@wip.dk", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private void SaveMenu(Menu menu)
        {
            var menu_operation = TableOperation.Insert(menu.MenuEntity);
            menu_table.Execute(menu_operation);

            var items_batch_operation = new TableBatchOperation();
            foreach (var item in menu.ItemEntities)
            {
                Trace.TraceInformation("Item id " + item.Id);
                items_batch_operation.Add(TableOperation.Insert(item));
            }
            item_table.ExecuteBatch(items_batch_operation);
        }
    }
}
