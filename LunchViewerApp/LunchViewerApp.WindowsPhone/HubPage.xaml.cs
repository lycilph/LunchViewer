using LunchViewerApp.Common;
using LunchViewerApp.Models;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Networking.PushNotifications;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace LunchViewerApp
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigation_helper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        
        public NavigationHelper NavigationHelper
        {
            get { return navigation_helper; }
        }

        public HubPage()
        {
            InitializeComponent();
            DataContext = this;

            //RegisterBackgroundTask();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            NavigationCacheMode = NavigationCacheMode.Required;

            navigation_helper = new NavigationHelper(this);
            navigation_helper.LoadState += this.NavigationHelper_LoadState;
            navigation_helper.SaveState += this.NavigationHelper_SaveState;
        }

        private async void RefreshMenus()
        {
            // This code refreshes the entries in the list view be querying the TodoItems table.
            // The query excludes completed TodoItems
            try
            {
                var menu_table = App.MobileService.GetTable<Menu>();
                var menus = await menu_table.ToListAsync();
                System.Diagnostics.Debug.WriteLine("Found {0} menus", menus.Count());
            }
            catch (MobileServiceInvalidOperationException)
            {
                //MessageBox.Show(e.Message, "Error loading items", MessageBoxButton.OK);
            }
        }

        private async void RegisterBackgroundTask()
        {
            var background_status = await BackgroundExecutionManager.RequestAccessAsync();

            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            await App.MobileService.GetPush().RegisterNativeAsync(channel.Uri);

            var task_name = "Raw notifications background task";
            var tasks = BackgroundTaskRegistration.AllTasks;
            foreach (var task in tasks)
            {
                if (task.Value.Name == task_name)
                {
                    task.Value.Unregister(true);
                    break;
                }
            }

            var builder = new BackgroundTaskBuilder();
            builder.Name = task_name;
            builder.TaskEntryPoint = "BackgroundTasks.NewDataBackgroundTask";
            var trigger = new Windows.ApplicationModel.Background.PushNotificationTrigger();
            builder.SetTrigger(trigger);
            BackgroundTaskRegistration task_registration = builder.Register();
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
        private /*async*/ void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            //this.DefaultViewModel["Groups"] = sampleDataGroups;

            RefreshMenus();
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
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Details about the click event.</param>
        private void GroupSection_ItemClick(object sender, ItemClickEventArgs e)
        {
        //    var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
        //    if (!Frame.Navigate(typeof(SectionPage), groupId))
        //    {
        //        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
        //    }
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The source of the click event.</param>
        /// <param name="e">Defaults about the click event.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
        //    var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
        //    if (!Frame.Navigate(typeof(ItemPage), itemId))
        //    {
        //        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
        //    }
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
            this.navigation_helper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigation_helper.OnNavigatedFrom(e);
        }

        #endregion
    }
}