using System;
using System.Threading.Tasks;
using System.Web.Http;
using LunchViewerService.DataObjects;
using LunchViewerService.Utils;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LunchViewerService.ScheduledJobs
{
    public class TestNotificationJob : ScheduledJob
    {
        private CloudTable error_table;

        public override async Task ExecuteAsync()
        {
            try
            {
                var storage_account = CloudStorageAccount.Parse(Services.Settings["StorageConnectionString"]);
                var table_client = storage_account.CreateCloudTableClient();

                error_table = table_client.GetTableReference("Errors");
                error_table.CreateIfNotExists();
                
                await NotificationsHelper.SendNotificationAsync(Services);
            }
            catch (Exception e)
            {
                var msg = string.Format("{0} {1}", e.GetType(), e.Message);
                AddError("General Error", msg);
                Services.Log.Info(msg);
            }
        }

        private void AddError(string type, string message)
        {
            var error = new ErrorEntity(type, message);
            var op = TableOperation.Insert(error);
            error_table.Execute(op);
        }
    }
}