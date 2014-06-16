using System;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace LunchViewerApp.Controllers
{
    public class BackgroundTaskController
    {
        public static async void RegisterBackgroundTasks()
        {
            var state = await BackgroundExecutionManager.RequestAccessAsync();
            if (state != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                throw new InvalidOperationException("Couldn't register background tasks");

            var update_tile_trigger = new TimeTrigger(60, false);
            Register("UpdateTileTask", update_tile_trigger);

            var update_push_channel_trigger = new MaintenanceTrigger(15, true);
            Register("UpdatePushChannelTask", update_push_channel_trigger);

            //var new_data_trigger = new PushNotificationTrigger();
            //Register("NewDataBackgroundTask", new_data_trigger);
        }

        private static void Register(string name, IBackgroundTrigger trigger)
        {
            // See if the task is already registered
            var tasks = BackgroundTaskRegistration.AllTasks;
            if (tasks.Any(task => task.Value.Name == name))
                return;

            // If the background task is not already registered, do it now
            var builder = new BackgroundTaskBuilder { Name = name, TaskEntryPoint = "BackgroundTasks." + name};
            builder.SetTrigger(trigger);
            builder.Register();
        }
    }
}
