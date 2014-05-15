using LunchViewerApp.Models;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace LunchViewerApp
{
    public static class MenuDownloader
    {
        public static async Task Execute()
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
            foreach (var item in items_to_download)
            {
                var week_number = Convert.ToInt32(item);
                await LoadAsync(week_number);
            }
        }

        private static async Task LoadAsync(int week)
        {
            try
            {
                var menus_table = App.MobileService.GetTable<Menu>();
                var menus = await menus_table.Where(m => m.Week == week).ToListAsync();

                if (menus.Any())
                {
                    var menu = menus.First();

                    var items_table = App.MobileService.GetTable<Item>();
                    var items = await items_table.Where(i => i.ParentId == menu.MenuId).ToListAsync();

                    var menu_container = ApplicationData.Current.LocalSettings.CreateContainer("menus", ApplicationDataCreateDisposition.Always); 
                    var serialized_menu = JsonConvert.SerializeObject(new { Menu = menu, Items = items });
                    menu_container.Values[week.ToString()] = serialized_menu;
                }
            }
            catch (MobileServiceInvalidOperationException e)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + e.Message);
            }
        }
    }
}
