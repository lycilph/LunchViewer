using CommonLibrary;
using Microsoft.WindowsAzure.MobileServices;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class NewDataBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;
            if (notification.Content == "NewData")
            {
                var MobileService = new MobileServiceClient("https://lunchviewerservice.azure-mobile.net/", "fkVMfCuWPoTLEorySMugByrbsZsVxA30");
                var data_found = await MenuDownloader.Execute(MobileService);

                if (data_found)
                {
                    // Create content
                    var toast_content = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
                    XmlNodeList toast_text_elements = toast_content.GetElementsByTagName("text");
                    toast_text_elements[0].AppendChild(toast_content.CreateTextNode("New data"));
                    toast_text_elements[1].AppendChild(toast_content.CreateTextNode("Launch LunchViewer"));
                    // Create the toast notification based on the XML content you've specified.
                    ToastNotification toast_notification = new ToastNotification(toast_content);
                    // Send your toast notification.
                    ToastNotificationManager.CreateToastNotifier().Show(toast_notification);

                    var badge_content = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
                    var badge_attributes = badge_content.GetElementsByTagName("badge");
                    var badge_xml_element = (XmlElement)badge_attributes[0];
                    badge_xml_element.SetAttribute("value", "alert");
                    // Create a badge notification from XML
                    var badge_notification = new BadgeNotification(badge_content);
                    // Send the badge notification to the app's tile.
                    BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge_notification);
                }
            }

            deferral.Complete();
        }
    }
}
