using Microsoft.WindowsAzure.MobileServices;

namespace LunchViewerApp.Core
{
    public static class MobileServiceUtils
    {
        public static MobileServiceClient Client = new MobileServiceClient("https://lunchviewerservice.azure-mobile.net/", "fkVMfCuWPoTLEorySMugByrbsZsVxA30");
        //public static MobileServiceClient Client = new MobileServiceClient("http://192.168.237.128:51031");
    }
}
