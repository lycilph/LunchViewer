using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceBus.Notifications;

namespace LunchViewerService.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/ResendNotification".

    public class ResendNotificationJob : ScheduledJob
    {
        public async override Task ExecuteAsync()
        {
            Services.Log.Info("Resending notification");

            var storage_account = CloudStorageAccount.Parse(Services.Settings["StorageConnectionString"]);
            var table_client = storage_account.CreateCloudTableClient();
            var menu_table = table_client.GetTableReference("Menus");

            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
            var query = new TableQuery<MenuEntity>().Where(filter);
            var result = menu_table.ExecuteQuery(query);
            var count = result.Count() + 1;

            Services.Log.Info(string.Format("Found {0} menus", count));

            var hub_connection_name = Services.Settings["MS_NotificationHubName"];
            var hub_connection_string = Services.Settings["MS_NotificationHubConnectionString"];
            var hub = NotificationHubClient.CreateClientFromConnectionString(hub_connection_string, hub_connection_name, true);
            
            var registrations = await hub.GetAllRegistrationsAsync(Int32.MaxValue);
            Services.Log.Info(string.Format("Found {0} registrations", registrations.Count()));

            if (registrations.Any())
            {
                var message = string.Format("Processed new menu ({0}, {1})", 2014, 20);
                var notification = new WindowsNotification(message);
                notification.Headers.Add("X-WNS-Type", "wns/raw");

                var outcome = await hub.SendNotificationAsync(notification);
                Services.Log.Info(string.Format("Notification outcome {0}", outcome.State));
            }

            //return Task.FromResult(true);
        }
    }
}