using Windows.ApplicationModel.Background;
using Core;
using Core.Controllers;

namespace BackgroundTasks
{
    public sealed class UpdatePushChannelTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("Update push notification channel (background)");

            var mobile_service = MobileServiceController.CreateMobileServiceClient();
            await PushNotificationController.UpdateChannelAsync(mobile_service);

            deferral.Complete();
        }
    }
}
