using CommonLibrary;
using CommonLibrary.Viewmodels;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class UpdateTileBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            await Logger.WriteAsync("Updating tiles");

            var menus = new MenuCollectionViewModel();
            menus.Update();

            var item = menus.GetNextItem();
            if (item == null)
                ClearTile();
            else
                UpdateTile(item);

            deferral.Complete();
        }


        private static void ClearTile()
        {
            var tile_update_manager = TileUpdateManager.CreateTileUpdaterForApplication();
            tile_update_manager.Clear();
        }

        private static void UpdateTile(ItemViewModel item)
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
