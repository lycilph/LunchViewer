using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceBus.Notifications;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.Utils
{
    public static class NotificationsHelper
    {
        public static async Task SendNotificationAsync(ApiServices services)
        {
            var hub_connection_name = services.Settings["MS_NotificationHubName"];
            var hub_connection_string = services.Settings["MS_NotificationHubConnectionString"];
            var hub = NotificationHubClient.CreateClientFromConnectionString(hub_connection_string, hub_connection_name, true);

            var registrations = await hub.GetAllRegistrationsAsync(Int32.MaxValue);
            if (registrations.Any())
            {
                var google_outcome = await hub.SendGcmNativeNotificationAsync("{\"data\" : { \"message\": \"NewData\" }}");
                services.Log.Info(string.Format("Notification outcome (google gcm) {0}", google_outcome.State));
            }
        }
    }
}
