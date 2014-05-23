using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace CommonLibrary
{
    public static class BackgroundTaskUtils
    {
        public static async void Initialize()
        {
            var state = await BackgroundExecutionManager.RequestAccessAsync();

            var time_trigger = new TimeTrigger(60, false);
            Register("UpdateTileBackgroundTask", time_trigger);

            var push_trigger = new PushNotificationTrigger();
            Register("NewDataBackgroundTask", push_trigger);

            var maintenance_trigger = new MaintenanceTrigger(15, true);
            Register("UpdatePushChannelBackgroundTask", maintenance_trigger);
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
