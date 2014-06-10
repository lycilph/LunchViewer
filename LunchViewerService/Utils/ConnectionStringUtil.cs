using Microsoft.WindowsAzure.Mobile.Service;

namespace LunchViewerService.Utils
{
    public static class ConnectionStringUtil
    {
        static bool connection_string_initialized = false;

        public static void InitializeConnectionString(string connection_string_name, ApiServices services)
        {
            if (!connection_string_initialized)
            {
                connection_string_initialized = true;
                if (!services.Settings.Connections.ContainsKey(connection_string_name))
                {
                    var connection_from_app_settings = services.Settings[connection_string_name];
                    var connection_setting = new ConnectionSettings(connection_string_name, connection_from_app_settings);
                    services.Settings.Connections.Add(connection_string_name, connection_setting);
                }
            }
        }
    }
}