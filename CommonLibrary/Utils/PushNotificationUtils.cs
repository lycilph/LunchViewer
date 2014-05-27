using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace CommonLibrary.Utils
{
    public static class PushNotificationUtils
    {
        public static async Task UpdateChannelAsync(MobileServiceClient mobile_service)
        {
            var message = string.Empty;
            try
            {
                await Logger.WriteAsync("UpdateChannelAsync");
                await Logger.WriteAsync("IsNetworkAvailable = " + IsNetworkAvailable());

                var current_push_channel = ApplicationData.Current.LocalSettings.Values["channel_uri"] as string;
                var new_push_channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                await Logger.WriteAsync("After CreatePushNotificationChannelForApplicationAsync");

                if (current_push_channel == null || current_push_channel != new_push_channel.Uri)
                {
                    ApplicationData.Current.LocalSettings.Values["channel_uri"] = new_push_channel.Uri;
                    await mobile_service.GetPush().RegisterNativeAsync(new_push_channel.Uri);
                    await Logger.WriteAsync("After RegisterNativeAsync");
                }
            }
            catch (Exception e)
            {
                message = string.Format("Couldn't register the app for push notifications ({0})", e.Message);
            }

            if (!string.IsNullOrEmpty(message))
                await Logger.WriteAsync(message);
        }

        public static bool IsNetworkAvailable()
        {
            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();

                if (profile == null)
                {
                    return false;
                }
                else
                {
                    var networkAdapterInfo = profile.NetworkAdapter;
                    if (networkAdapterInfo == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                throw;
#endif
                return false;
            }
        }
    }
}
