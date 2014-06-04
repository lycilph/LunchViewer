using LunchViewerApp.Core.Data;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace LunchViewerApp.Core
{
    public static class ModelUtils
    {
        // LoadMenu
        // SaveMenu

        public static async Task<bool> DownloadMenusAsync()
        {
            await Logger.WriteAsync("Downloading menus");

            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
            var weeks = WeekUtils.Weeks.AsStrings();

            // Find and remove old data
            var items_to_remove = menu_container.Values.Where(i => !weeks.Contains(i.Key)).Select(i => i.Key);
            foreach (var item in items_to_remove)
                menu_container.Values.Remove(item);

            // Download new data
            var items_to_download = weeks.Where(n => menu_container.Values[n] == null);
            var result = false;
            foreach (var item in items_to_download)
            {
                var week_result = await DownloadMenuAsync(item);
                if (week_result)
                    result = true;
            }

            return result;
        }

        private static async Task<bool> DownloadMenuAsync(string week)
        {
            var message = string.Empty;
            try
            {
                var menus_table = MobileServiceUtils.Client.GetTable<MenuData>();

                var week_number = Convert.ToInt32(week);
                var menus = await menus_table.Where(m => m.Week == week_number).ToListAsync();

                if (menus.Any())
                {
                    var menu = menus.First();

                    var items_table = MobileServiceUtils.Client.GetTable<ItemData>();
                    var items = await items_table.Where(i => i.ParentId == menu.MenuId).ToListAsync();

                    var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
                    var serialized_menu = JsonConvert.SerializeObject(new { Menu = menu, Items = items });
                    menu_container.Values[week.ToString()] = serialized_menu;

                    return true;
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                message = "Error: " + e.Message;
            }

            if (!string.IsNullOrWhiteSpace(message))
                await Logger.WriteAsync(message);

            return false;
        }
    }
}
