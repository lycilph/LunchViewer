using CommonLibrary.Models;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace CommonLibrary.Utils
{
    public static class DownloadUtils
    {
        public static async Task<bool> DownloadMenusAsync(MobileServiceClient mobile_service)
        {
            var week_numbers = new List<int> { WeekUtils.PreviousWeekNumber, WeekUtils.CurrentWeekNumber, WeekUtils.NextWeekNumber };
            var week_number_strings = week_numbers.Select(n => n.ToString());

            var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always);

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

        private static async Task<bool> DownloadMenuAsync(MobileServiceClient mobile_service, int week)
        {
            try
            {
                var menus_table = mobile_service.GetTable<Menu>();
                var menus = await menus_table.Where(m => m.Week == week).ToListAsync();

                if (menus.Any())
                {
                    var menu = menus.First();

                    var items_table = mobile_service.GetTable<Item>();
                    var items = await items_table.Where(i => i.ParentId == menu.MenuId).ToListAsync();

                    var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always); 
                    var serialized_menu = JsonConvert.SerializeObject(new { Menu = menu, Items = items });
                    menu_container.Values[week.ToString()] = serialized_menu;

                    return true;
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                Debug.WriteLine("Error: " + e.Message);
            }

            return false;
        }
    }
}
