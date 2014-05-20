using LunchViewerApp.Common;
using LunchViewerApp.ViewModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace LunchViewerApp
{
    public sealed partial class HubPage : Page
    {
        private readonly MainViewModel view_model;
        private readonly NavigationHelper navigation_helper;
        private readonly ResourceLoader resource_loader = ResourceLoader.GetForCurrentView("Resources");

        public NavigationHelper NavigationHelper
        {
            get { return navigation_helper; }
        }

        public ICommand UpdateMenusCommand { get; set; }

        public HubPage()
        {
            InitializeComponent();

            view_model = new MainViewModel();
            DataContext = view_model;

            RegisterBackgroundTask();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            NavigationCacheMode = NavigationCacheMode.Required;

            navigation_helper = new NavigationHelper(this);
            navigation_helper.LoadState += NavigationHelper_LoadState;
            navigation_helper.SaveState += NavigationHelper_SaveState;
        }
        
        private async void RegisterBackgroundTask()
        {
            var background_status = await BackgroundExecutionManager.RequestAccessAsync();

            await UpdatePushNotificationChannel();

            // See if the task is already registered
            var task_name = "NewDataBackgroundTask";
            var tasks = BackgroundTaskRegistration.AllTasks;
            foreach (var task in tasks)
            {
                if (task.Value.Name == task_name)
                {
                    // DEBUG
                    task.Value.Unregister(false);
                    break;

                    //return;
                }
            }

            // If the background task is not already registered, do it now
            var builder = new BackgroundTaskBuilder();
            builder.Name = task_name;
            builder.TaskEntryPoint = "BackgroundTasks.NewDataBackgroundTask";
            var trigger = new PushNotificationTrigger();
            builder.SetTrigger(trigger);
            BackgroundTaskRegistration task_registration = builder.Register();
        }

        private static async Task UpdatePushNotificationChannel()
        {
            var update_channel = true;

            var new_push_channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            var current_push_channel = ApplicationData.Current.LocalSettings.Values["channel_uri"] as string;
            if (current_push_channel != null && current_push_channel == new_push_channel.Uri)
                update_channel = false;

            if (update_channel)
            {
                ApplicationData.Current.LocalSettings.Values["channel_uri"] = new_push_channel.Uri;

                MessageDialog dialog = null;
                try
                {
                    await App.MobileService.GetPush().RegisterNativeAsync(new_push_channel.Uri);
                }
                catch (Exception)
                {
                    dialog = new MessageDialog("Couldn't register the app for push notifications", "Error");
                }

                if (dialog != null)
                    await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            view_model.LoadState();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            ShowItem(e.ClickedItem as ItemViewModel);
        }

        private void NextItemClick(object sender, RoutedEventArgs e)
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
            var first_section = hub_control.Sections[0];
            hub_control.ScrollToSection(first_section);
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
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