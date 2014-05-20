using CommonLibrary;
using Microsoft.WindowsAzure.MobileServices;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.Storage;

namespace BackgroundTasks
{
    public sealed class NewDataBackgroundTask : IBackgroundTask
    {
        private static MobileServiceClient MobileService = new MobileServiceClient("https://lunchviewerservice.azure-mobile.net/", "fkVMfCuWPoTLEorySMugByrbsZsVxA30");

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;
            if (notification.Content == "NewData")
                await MenuDownloader.Execute(MobileService);

            deferral.Complete();
        }
    }
}
