using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.Controllers
{
    public class MailController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/Mail
        public HttpResponseMessage Post()
        {
            Services.Log.Info(string.Format("Hello from custom controller! ({0})", Request.Content));
            return Request.CreateResponse(HttpStatusCode.OK, "Message received");
        }
    }
}
