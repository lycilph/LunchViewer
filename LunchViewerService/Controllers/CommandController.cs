using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using LunchViewerService.Utils;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace LunchViewerService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class CommandController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/Mail
        public async Task<HttpResponseMessage> Post(string command)
        {
            Services.Log.Info(string.Format("Received a mail"));

            if (command != "SendNotification")
                return Request.CreateBadRequestResponse("Unknown command: " + command);

            try
            {
                await NotificationsHelper.SendNotificationAsync(Services);
                return Request.CreateResponse(HttpStatusCode.OK, "Command accepted");
            }
            catch (Exception e)
            {
                Services.Log.Info(string.Format("{0} {1}", e.GetType(), e.Message));
                return Request.CreateBadRequestResponse(e.Message);
            }
        }
    }
}
