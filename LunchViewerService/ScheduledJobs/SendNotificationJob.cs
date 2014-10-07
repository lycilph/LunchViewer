using System;
using System.Threading.Tasks;
using System.Web.Http;
using LunchViewerService.Utils;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/SendNotification".

    public class SendNotificationJob : ScheduledJob
    {
        public override async Task ExecuteAsync()
        {
            try
            {
                await NotificationsHelper.SendNotificationAsync(Services);
            }
            catch (Exception e)
            {
                Services.Log.Info(string.Format("{0} {1}", e.GetType(), e.Message));
            }
        }
    }
}