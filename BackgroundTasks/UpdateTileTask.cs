using Windows.Data.Xml.Dom;
using Core;
using Core.Controllers;
using Core.Models;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class UpdateTileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("Updating tiles");

            Item next_item = null;
            var next_lunch_date = DateUtils.GetNextLunchDate();

            var current_week = MenuController.ReadMenu(DateUtils.CurrentWeekNumber);
            if (current_week != null)
                next_item = current_week.Items.FirstOrDefault(item => item.Date.Date == next_lunch_date.Date);

            if (next_item == null)
            {
                var next_week = MenuController.ReadMenu(DateUtils.NextWeekNumber);
                if (next_week != null && next_week.Items.Any())
                    next_item = next_week.Items.First();
            }

            if (next_item == null)
                ClearTile();
            else
                UpdateTile(next_item);

            deferral.Complete();
        }

        private static void ClearTile()
        {
            var tile_update_manager = TileUpdateManager.CreateTileUpdaterForApplication();
            tile_update_manager.Clear();
        }

        private static void UpdateTile(Item item)
        {
            var tile_update_manager = TileUpdateManager.CreateTileUpdaterForApplication();
            tile_update_manager.Clear();

            // Wide text tile
            var tile_content = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText02);
            var tile_text = tile_content.GetElementsByTagName("text");
            tile_text[0].InnerText = item.Date.ToString("d/M");
            tile_text[2].InnerText = item.Text;
            var tile_notification = new TileNotification(tile_content);
            tile_update_manager.Update(tile_notification);

            // Wide image tile
            tile_content = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Image);
            var tile_image = tile_content.GetElementsByTagName("image");
            ((XmlElement)tile_image[0]).SetAttribute("src", "ms-appx:///Assets/WideLogo.png");
            tile_notification = new TileNotification(tile_content);
            tile_update_manager.Update(tile_notification);

            // Square image and text tile
            tile_content = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText02);
            tile_image = tile_content.GetElementsByTagName("image");
            ((XmlElement)tile_image[0]).SetAttribute("src", "ms-appx:///Assets/Logo.png");
            tile_text = tile_content.GetElementsByTagName("text");
            tile_text[0].InnerText = item.Date.ToString("d/M");
            tile_text[1].InnerText = item.Text;
            tile_notification = new TileNotification(tile_content);
            tile_update_manager.Update(tile_notification);
        }
    }
}
