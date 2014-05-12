using Microsoft.ServiceBus.Notifications;
using Microsoft.WindowsAzure.ServiceRuntime;
using Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EmailProcessor
{
    public static class NotificationsHelper
    {
        public static async Task SendNotificationAsync(Menu menu)
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

        public static void SendNotification(Menu menu)
        {
            SendNotificationAsync(menu).Wait();
        }
    }
}
