using Windows.UI.Notifications;

namespace LunchViewerApp.Controllers
{
    public class NotificationsController
    {
        public static void InitializeNotifications()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueueForWide310x150(true);
        }
    }
}