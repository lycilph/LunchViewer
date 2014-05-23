using GalaSoft.MvvmLight;
using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace LunchViewerApp.ViewModels
{
    public class NextItemViewModel : ViewModelBase
    {
        private string _Header;
        public string Header
        {
            get { return _Header; }
            set { Set(() => Header, ref _Header, value); }
        }

        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set { Set(() => Date, ref _Date, value); }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { Set(() => Text, ref _Text, value); }
        }

        private ItemViewModel _ItemViewModel;
        public ItemViewModel ItemViewModel
        {
            get { return _ItemViewModel; }
            set { Set(() => ItemViewModel, ref _ItemViewModel, value); }
        }
        
        public void Update(ItemViewModel item)
        {
            if (item == null)
            {
                Header = "Next lunch";
            }
            else if (item.IsToday())
                Header = "Today";
            else if (item.IsTomorrow())
                Header = "Tomorrow";
            else
                Header = item.Date.ToString("dddd");

            ItemViewModel = item;
            if (item != null)
            {
                Date = item.Date;
                Text = item.Text;

                UpdateTile();
            }
            else
            {
                Date = DateTime.Now;
                Text = "None";
            }
        }

        private void UpdateTile()
        {
            var tile_update_manager = TileUpdateManager.CreateTileUpdaterForApplication();
            tile_update_manager.Clear();

            // Wide text tile
            var tile_content = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText02);
            var tile_text = tile_content.GetElementsByTagName("text");
            tile_text[0].InnerText = Date.ToString("d/M");
            tile_text[2].InnerText = Text;
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
            tile_text[0].InnerText = Date.ToString("d/M");
            tile_text[1].InnerText = Text;
            tile_notification = new TileNotification(tile_content);
            tile_update_manager.Update(tile_notification);
        }
    }
}
