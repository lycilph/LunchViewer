using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using Core;
using Core.Controllers;

namespace BackgroundTasks
{
    public sealed class NewDataTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("New data notification received");

            var notification = (RawNotification)taskInstance.TriggerDetails;
            var content = notification.Content.Trim();
            if (content == "NewData")
            {
                var mobile_service = MobileServiceController.CreateMobileServiceClient();
                var data_found = await MenuController.UpdateMenusAsync(mobile_service);

                if (data_found)
                {
                    await Logger.WriteAsync("New data downloaded");
                    ShowToastNotification();
                    ShowBadgeNotification();
                }
            }

            deferral.Complete();
        }

        private static void ShowBadgeNotification()
        {
            // Create badge content
            var badge_content = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            var badge_attributes = badge_content.GetElementsByTagName("badge");
            var badge_xml_element = (XmlElement)badge_attributes[0];
            badge_xml_element.SetAttribute("value", "alert");
            // Create a badge notification from XML
            var badge_notification = new BadgeNotification(badge_content);
            // Send the badge notification to the app's tile.
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge_notification);
        }

        private static void ShowToastNotification()
        {
            // Create toast content
            var toast_content = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
            var toast_text_elements = toast_content.GetElementsByTagName("text");
            toast_text_elements[0].AppendChild(toast_content.CreateTextNode("New data"));
            toast_text_elements[1].AppendChild(toast_content.CreateTextNode("Launch LunchViewer"));
            // Create the toast notification based on the XML
            var toast_notification = new ToastNotification(toast_content);
            // Send your toast notification.
            ToastNotificationManager.CreateToastNotifier().Show(toast_notification);
        }
    }
}
