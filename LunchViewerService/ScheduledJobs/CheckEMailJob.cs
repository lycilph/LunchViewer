using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using LunchViewerService.DataObjects;
using LunchViewerService.Models;
using LunchViewerService.Utils;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/CheckEmail".

    public class CheckEmailJob : ScheduledJob
    {
        public override async Task ExecuteAsync()
        {
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
                            var context = new LunchViewerContext();

                            // Get message indices and set messages read flag
                            var indices = unread_mails.Select(s => s.Index).ToArray();
                            inbox.AddFlags(indices, MessageFlags.Seen, true, cancel.Token);

                            var found_emails = false;

                            foreach (var summary in unread_mails)
                            {
                                var message = inbox.GetMessage(summary.Index);

                                if (EmailHelper.IsValidMenuMail(message))
                                {
                                    Services.Log.Info(string.Format("[Mail] {0:D2}: {1}", summary.Index, message.Subject));

                                    Menu menu;
                                    if (EmailHelper.TryParseMessage(message, out menu))
                                    {
                                        context.Menus.Add(menu);
                                        found_emails = true;
                                    }
                                }
                                else
                                {
                                    Services.Log.Info(string.Format("Not a valid menu mail (subject {0})", message.Subject));
                                }
                            }

                            if (found_emails)
                            {
                                await context.SaveChangesAsync(cancel.Token);
                                await NotificationsHelper.SendNotificationAsync(Services);                                
                            }
                        }

                        client.Disconnect(true, cancel.Token);
                    }
                }
            }
            catch (Exception e)
            {
                Services.Log.Error(string.Format("{0} {1}", e.GetType(), e.Message));
            }
        }
    }
}