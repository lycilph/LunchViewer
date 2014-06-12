using LunchViewerApp.Common;
using System;
using Windows.UI.Xaml.Navigation;
using LunchViewerApp.ViewModels;

namespace LunchViewerApp
{
    public sealed partial class ItemPage
    {
        private readonly NavigationHelper navigation_helper;

        public ItemPage()
        {
            InitializeComponent();

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
            var context = e.NavigationParameter as ItemViewModel;
            if (context == null)
                throw new InvalidOperationException("NavigationParameter must be an ItemViewModel");

            DataContext = context;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigation_helper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigation_helper.OnNavigatedFrom(e);
        }
    }
}
