using Windows.ApplicationModel.Background;

namespace BackgroundTasks
{
    public sealed class UpdatePushChannelBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            System.Diagnostics.Debug.WriteLine("UpdatePushChannelBackgroundTask");

            //deferral.Complete();
        }
    }
}
