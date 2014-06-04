using LunchViewerApp.Core;
using Windows.ApplicationModel.Background;

namespace BackgroundTasks
{
    public sealed class UpdateTilesTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("Updating tiles");

            deferral.Complete();
        }
    }
}
