using LunchViewerService.DataObjects;
using LunchViewerService.Models;
using LunchViewerService.Utils;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchViewerService.ScheduledJobs
{
    // A scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/CheckEMail".

    public class CheckEMailJob : ScheduledJob
    {
        private CloudTable menu_table;
        private CloudTable item_table;
        private CloudTable error_table;

        public async override Task ExecuteAsync()
        {
            GetOrCreateTables();

            try
            {
                using (var client = new ImapClient())
                {
                    var aes = new SimpleAES();
                    var email_address = aes.Decrypt("1yXS2FR8fVtTXD/WUuigNrq2zwcXVb9yL1xxCX2ufIpyBI6vg8cgKByASwm8h9nm");
                    var email_password = aes.Decrypt("VyE2LIjIhRIQLl1VvkAknw==");

                    var credentials = new NetworkCredential(email_address, email_password);
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

                        // Get unread messages
                        var unread_mails = inbox.Fetch(0, -1, MessageSummaryItems.Flags).Where(s => s.Flags.HasValue && !s.Flags.Value.HasFlag(MessageFlags.Seen)).ToList();
                        Services.Log.Info(string.Format("Unread messages: {0}", unread_mails.Count()));

                        if (unread_mails.Any())
                        {
                            // Get message indices and set messages read flag
                            var indices = unread_mails.Select(s => s.Index).ToArray();
                            inbox.AddFlags(indices, MessageFlags.Seen, true, cancel.Token);

                            var send_notification = false;

                            foreach (var summary in unread_mails)
                            {
                                var message = inbox.GetMessage(summary.Index);

                                if (EmailHelper.IsValidMenuMail(message))
                                {
                                    Services.Log.Info(string.Format("[Mail] {0:D2}: {1}", summary.Index, message.Subject));

                                    Menu menu;
                                    if (EmailHelper.TryParseMessage(message, out menu))
                                    {
                                        SaveMenu(menu);
                                        send_notification = true;
                                    }
                                }
                                else
                                {
                                    var error_message = string.Format("Not a valid menu mail (subject {0})", message.Subject);

                                    AddError("Invalid Mail", error_message);
                                    Services.Log.Info(error_message);
                                }
                            }

                            if (send_notification)
                                await NotificationsHelper.SendNotificationAsync(Services);
                        }

                        client.Disconnect(true, cancel.Token);
                    }
                }
            }
            catch (Exception e)
            {
                AddError("General Error", string.Format("{0} {1}", e.GetType(), e.Message));
            }
        }

        private void GetOrCreateTables()
        {
            var storage_account = CloudStorageAccount.Parse(Services.Settings["StorageConnectionString"]);
            var table_client = storage_account.CreateCloudTableClient();

            menu_table = table_client.GetTableReference("Menus");
            menu_table.CreateIfNotExists();

            item_table = table_client.GetTableReference("Items");
            item_table.CreateIfNotExists();

            error_table = table_client.GetTableReference("Errors");
            error_table.CreateIfNotExists();
        }

        private void AddError(string type, string message)
        {
            var error = new ErrorEntity(type, message);
            var op = TableOperation.Insert(error);
            error_table.Execute(op);
        }

        private void SaveMenu(Menu menu)
        {
            var menu_operation = TableOperation.Insert(menu.MenuEntity);
            menu_table.Execute(menu_operation);

            var items_batch_operation = new TableBatchOperation();
            menu.ItemEntities.Apply(i => items_batch_operation.Add(TableOperation.Insert(i)));

            item_table.ExecuteBatch(items_batch_operation);
        }
    }
}