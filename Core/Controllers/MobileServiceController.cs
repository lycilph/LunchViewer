using Microsoft.WindowsAzure.MobileServices;

namespace Core.Controllers
{
    public static class MobileServiceController
    {
        public static MobileServiceClient CreateMobileServiceClient()
        {
            // Production service
            return new MobileServiceClient("https://lunchviewer.azure-mobile.net/", "SVzovNQtJGFXALLJDUskHXIZqDSBwL46");
            
            // Debug service
            //return new MobileServiceClient("http://192.168.237.128:51031");
        }
    }
}
