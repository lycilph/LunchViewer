using Windows.ApplicationModel.Background;

namespace BackgroundTasks
{
    public sealed class UpdateTileBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            System.Diagnostics.Debug.WriteLine("UpdateTileBackgroundTask");

            //deferral.Complete();
        }
    }
}
