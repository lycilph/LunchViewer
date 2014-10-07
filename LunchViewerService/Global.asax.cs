using System.Web;

namespace LunchViewerService
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();
        }
    }
}