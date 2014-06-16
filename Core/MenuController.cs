using Core.Models;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Core
{
    public static class MenuController
    {
        public static Week ReadMenu(int week)
        {
            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
            var serialized_menu = menu_container.Values[week.ToString()] as string;
            if (serialized_menu == null)
                return null;

            return JsonConvert.DeserializeObject<Week>(serialized_menu);
        }

        public static async Task<bool> UpdateMenusAsync(MobileServiceClient mobile_service)
        {
            await Logger.WriteAsync("Updating menus");

            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
            var week_number_strings = DateUtils.Weeks.Select(n => n.ToString());

            // Find and remove old data
            var items_to_remove = menu_container.Values.Where(i => !week_number_strings.Contains(i.Key)).Select(i => i.Key);
            foreach (var item in items_to_remove)
                menu_container.Values.Remove(item);

            // Download new data
            var items_to_download = week_number_strings.Where(n => menu_container.Values[n] == null);
            var result = false;
            foreach (var item in items_to_download)
            {
                var week_number = Convert.ToInt32(item);
                var week_result = await DownloadMenuAsync(mobile_service, week_number);
                if (week_result)
                    result = true;
            }

            return result;
        }

        private static async Task<bool> DownloadMenuAsync(MobileServiceClient mobile_service, int week_number)
        {
            var error_message = string.Empty;
            try
            {
                var menus_table = mobile_service.GetTable<Menu>();
                var menus = await menus_table.Where(m => m.Week == week_number).ToListAsync();

                if (menus.Any())
                {
                    var menu = menus.First();

                    await Logger.WriteAsync(string.Format("Downloading menu for week {0}", menu.Week));

                    var items_table = mobile_service.GetTable<Item>();
                    var items = await items_table.Where(i => i.ParentId == menu.MenuId).ToListAsync();

                    var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);
                    var serialized_menu = JsonConvert.SerializeObject(new Week(menu, items));
                    menu_container.Values[week_number.ToString()] = serialized_menu;

                    return true;
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                error_message = "Error: " + e.Message;
            }

            if (!string.IsNullOrWhiteSpace(error_message))
                await Logger.WriteAsync(error_message);

            return false;
        }
    }
}
