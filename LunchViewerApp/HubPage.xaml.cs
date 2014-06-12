using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using LunchViewerApp.Common;
using LunchViewerApp.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Navigation;

namespace LunchViewerApp
{
    public sealed partial class HubPage
    {
        private readonly HubViewModel view_model = new HubViewModel();
        private readonly NavigationHelper navigation_helper;
        private readonly ResourceLoader resource_loader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            InitializeComponent();
            DataContext = view_model;

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            NavigationCacheMode = NavigationCacheMode.Required;

            navigation_helper = new NavigationHelper(this);
            navigation_helper.LoadState += NavigationHelper_LoadState;
            navigation_helper.SaveState += NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return navigation_helper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            view_model.LoadState();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            ShowItem(e.ClickedItem as ItemViewModel);
        }

        private void OnNextItemClick(object sender, RoutedEventArgs e)
        {
            ShowItem(view_model.NextItem.ItemViewModel);
        }

        private void ShowItem(ItemViewModel item)
        {
            if (item == null)
                return;

            if (!Frame.Navigate(typeof(ItemPage), item))
                throw new Exception(resource_loader.GetString("NavigationFailedExceptionMessage"));
        }

        private void HomeClick(object sender, RoutedEventArgs e)
        {
            var first_section = HubControl.Sections[0];
            HubControl.ScrollToSection(first_section);
        }

        private void ShowLogClick(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(LogPage)))
                throw new Exception(resource_loader.GetString("NavigationFailedExceptionMessage"));
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigation_helper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigation_helper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
