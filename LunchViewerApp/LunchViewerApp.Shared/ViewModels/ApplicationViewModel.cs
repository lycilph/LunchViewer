using LunchViewerApp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace LunchViewerApp.ViewModels
{
    public class ApplicationViewModel
    {
        public async void Initialize()
        {
            await ModelUtils.DownloadMenusAsync();
            await InitializeBackgroundTasks();
        }

        private async Task InitializeBackgroundTasks()
        {
            var state = await BackgroundExecutionManager.RequestAccessAsync();

            var update_tiles_trigger = new TimeTrigger(60, false);
            Register("UpdateTilesTask", update_tiles_trigger);

            //var update_tile_trigger = new TimeTrigger(60, false);
            //Register("UpdateTileBackgroundTask", update_tile_trigger);

            //var new_data_trigger = new PushNotificationTrigger();
            //Register("NewDataBackgroundTask", new_data_trigger);

            //var update_push_channel_trigger = new MaintenanceTrigger(15, true);
            //Register("UpdatePushChannelBackgroundTask", update_push_channel_trigger);
        }

        private static void Register(string name, IBackgroundTrigger trigger)
        {
            // See if the task is already registered
            var tasks = BackgroundTaskRegistration.AllTasks;
            foreach (var task in tasks)
            {
                if (task.Value.Name == name)
                    return;
            }

            // If the background task is not already registered, do it now
            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = "BackgroundTasks." + name;
            builder.SetTrigger(trigger);
            BackgroundTaskRegistration task_registration = builder.Register();
        }
    }
}
