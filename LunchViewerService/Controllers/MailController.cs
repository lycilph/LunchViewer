using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using LunchViewerService.DataObjects;
using LunchViewerService.Models;
using LunchViewerService.Utils;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace LunchViewerService.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.Anonymous)]
    public class MailController : ApiController
    {
        private LunchViewerContext context;

        public ApiServices Services { get; set; }

        protected override void Initialize(HttpControllerContext controller_context)
        {
            base.Initialize(controller_context);
            context = new LunchViewerContext();
        }

        // POST api/Mail
        public async Task<HttpResponseMessage> Post()
        {
            Services.Log.Info(string.Format("Received a mail"));

            var message = await Request.Content.ReadAsStringAsync();
            Menu menu;
            if (!EmailHelper.TryParseMessage(Services, message, out menu))
                return Request.CreateBadRequestResponse("Could not parse message");

            if (context.Menus.FirstOrDefault(m => m.Year == menu.Year && m.Week == menu.Week) != null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "Menu already exists");

            Services.Log.Info(string.Format("Found menu for week {0} of {1}", menu.Week, menu.Year));

            context.Menus.Add(menu);
            await context.SaveChangesAsync();
            await NotificationsHelper.SendNotificationAsync(Services);           

            return Request.CreateResponse(HttpStatusCode.OK, "Message received and processed");
        }
    }
}
