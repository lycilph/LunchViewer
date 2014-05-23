using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace CommonLibrary
{
    public static class PushNotificationUtils
    {
        public static async Task UpdateChannelAsync(MobileServiceClient mobile_service)
        {
            var message = string.Empty;
            try
            {
                var current_push_channel = ApplicationData.Current.LocalSettings.Values["channel_uri"] as string;
                var new_push_channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                if (current_push_channel == null || current_push_channel != new_push_channel.Uri)
                {
                    ApplicationData.Current.LocalSettings.Values["channel_uri"] = new_push_channel.Uri;
                    await mobile_service.GetPush().RegisterNativeAsync(new_push_channel.Uri);
                }
            }
            catch (Exception e)
            {
                message = string.Format("Couldn't register the app for push notifications ({0})", e.Message);
            }

            if (!string.IsNullOrEmpty(message))
                await Logger.WriteAsync(message);
        }
    }
}
