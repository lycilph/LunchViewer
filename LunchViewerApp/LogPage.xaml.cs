using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using LunchViewerApp.Common;

namespace LunchViewerApp
{
    public sealed partial class LogPage
    {
        private readonly NavigationHelper navigation_helper;

        public ObservableCollection<string> Lines
        {
            get { return (ObservableCollection<string>)GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }
        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register("Lines", typeof(ObservableCollection<string>), typeof(LogPage), new PropertyMetadata(null));

        public LogPage()
        {
            InitializeComponent();
            DataContext = this;

            navigation_helper = new NavigationHelper(this);
            navigation_helper.LoadState += NavigationHelper_LoadState;
            navigation_helper.SaveState += NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return navigation_helper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Lines = new ObservableCollection<string>(await Logger.ReadAsync());
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private async void ClearLogClick(object sender, RoutedEventArgs e)
        {
            await Logger.ClearAsync();
            Lines.Clear();
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
