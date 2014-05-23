using Microsoft.WindowsAzure.MobileServices;

namespace CommonLibrary
{
    public static class MobileServiceUtils
    {
        public static MobileServiceClient CreateMobileServiceClient()
        {
            // Production service
            return new MobileServiceClient("https://lunchviewerservice.azure-mobile.net/", "fkVMfCuWPoTLEorySMugByrbsZsVxA30");
        
            // Debug service
            //return new MobileServiceClient("http://192.168.237.128:51031");
        }
    }
}
