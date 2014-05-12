using MailKit;
using MailKit.Net.Imap;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

namespace EmailProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        private CloudTable menu_table;
        private CloudTable item_table;
        private CloudTable error_table;

        public override void Run()
        {
            Trace.TraceInformation("EmailProcessor entry point called");

            while (true)
            {
                Trace.TraceInformation("Checking mail");
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

                                    if (EmailHelper.IsValidMenuMail(message))
                                    {
                                        Trace.TraceInformation("[Mail] {0:D2}: {1}", summary.Index, message.Subject);

                                        Menu menu;
                                        if (EmailHelper.TryParseMessage(message, out menu))
                                        {
                                            SaveMenu(menu);
                                            NotificationsHelper.SendNotification(menu);
                                        }
                                    }
                                    else
                                    {
                                        var error_message = string.Format("Not a valid menu mail (subject {0})", message.Subject);

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
            foreach (var item in menu.ItemEntities)
            {
                Trace.TraceInformation("Item id " + item.Id);
                items_batch_operation.Add(TableOperation.Insert(item));
            }
            item_table.ExecuteBatch(items_batch_operation);
        }
    }
}
