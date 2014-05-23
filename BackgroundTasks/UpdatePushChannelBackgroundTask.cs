using CommonLibrary;
using Windows.ApplicationModel.Background;

namespace BackgroundTasks
{
    public sealed class UpdatePushChannelBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("Updating push channel uri");

            var mobile_service = MobileServiceUtils.CreateMobileServiceClient();
            await PushNotificationUtils.UpdateChannelAsync(mobile_service);
            
            deferral.Complete();
        }
    }
}
