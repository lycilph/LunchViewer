using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LunchViewerService.ScheduledJobs
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/CheckEMail".

    public class CheckEMailJob : ScheduledJob
    {
        private CloudTable menu_table;
        private CloudTable item_table;
        private CloudTable error_table;

        public override Task ExecuteAsync()
        {
            Services.Log.Info("Hello from scheduled job!");
            return Task.FromResult(true);
        }

        private void GetOrCreateTables()
        {
            var storage_account = CloudStorageAccount.Parse(Services.Settings["StorageConnectionString"]);
            var table_client = storage_account.CreateCloudTableClient();

            menu_table = table_client.GetTableReference("Menus");
            menu_table.CreateIfNotExists();

            item_table = table_client.GetTableReference("Items");
            item_table.CreateIfNotExists();

            error_table = table_client.GetTableReference("Errors");
            error_table.CreateIfNotExists();
        }
    }
}