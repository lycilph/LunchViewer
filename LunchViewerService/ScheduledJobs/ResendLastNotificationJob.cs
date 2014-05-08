using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Table;
using ModelLibrary;
using System;
using System.Linq;
using Microsoft.ServiceBus.Notifications;

namespace LunchViewerService
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/ResendLastNotification".

    public class ResendLastNotificationJob : ScheduledJob
    {
        public override async Task ExecuteAsync()
        {
            Services.Log.Info("Hello from scheduled job!");

            string connection_string;
            Services.Settings.TryGetValue("StorageConnectionString", out connection_string);

            var storage_account = CloudStorageAccount.Parse(connection_string);
            var table_client = storage_account.CreateCloudTableClient();

            var menu_table = table_client.GetTableReference("Menus");
            if (menu_table.Exists())
            {
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
                var query = new TableQuery<MenuEntity>().Where(filter);
                var result = menu_table.ExecuteQuery(query)
                                       .OrderByDescending(m => m.Week)
                                       .First();

                Services.Log.Info(string.Format("Last menu found ({0} - {1})", result.Year, result.Week));
                await SendNotification(result);
            }
        }

        private async Task SendNotification(MenuEntity menu)
        {
            string hub_name;
            string hub_connection_string;
            Services.Settings.TryGetValue("MS_NotificationHubName", out hub_name);
            Services.Settings.TryGetValue("MS_NotificationHubConnectionString", out hub_connection_string);
            var hub = NotificationHubClient.CreateClientFromConnectionString(hub_connection_string, hub_name, true);

            var registrations = await hub.GetAllRegistrationsAsync(Int32.MaxValue);
            Services.Log.Info(string.Format("Found {0} registrations", registrations.Count()));

            if (registrations.Any())
            {
                var message = string.Format("Processed new menu ({0}, {1})", menu.Year, menu.Week);
                var notification = new WindowsNotification(message);
                notification.Headers.Add("X-WNS-Type", "wns/raw");

                var outcome = await hub.SendNotificationAsync(notification);
                Services.Log.Info(string.Format("Notification outcome {0}", outcome.State));
            }
        }
    }
}