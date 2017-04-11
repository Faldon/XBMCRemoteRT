using System.Threading.Tasks;
using Windows.UI.Popups;
using XBMCRemoteRT.Common;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using XBMCRemoteRT.Helpers;
using XBMCRemoteRT.Pages;
using XBMCRemoteRT.RPCWrappers;
using XBMCRemoteRT.Models.Network;
using Windows.ApplicationModel.Resources;
using Windows.System;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237


namespace XBMCRemoteRT
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private enum PageStates { Ready, Connecting }

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        ResourceLoader loader = new ResourceLoader();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            if (App.IsExpired()) {
                var alert = new MessageDialog(loader.GetString("TrialExpired"));
                alert.Commands.Add(new UICommand(loader.GetString("Buy"), async (cmd) => { await Launcher.LaunchUriAsync(Windows.ApplicationModel.Store.CurrentApp.LinkUri); }));
                alert.Commands.Add(new UICommand(loader.GetString("Close"), (cmd) => { App.Current.Exit(); }));
                alert.CancelCommandIndex = 1;
                await alert.ShowAsync();
                Frame.BackStack.Clear();
            }

            await LoadConnections();

            bool isAutoConnectEnabled = (bool)SettingsHelper.GetValue("AutoConnect", true);

            SetPageState(PageStates.Ready);
            Frame.BackStack.Clear();
            bool tryAutoLoad = e.Parameter as bool? ?? true;

            if (e.NavigationMode != NavigationMode.Back && isAutoConnectEnabled && tryAutoLoad) {
                ConnnectToRecentIp();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async Task LoadConnections()
        {
            await App.ConnectionsVM.ReloadConnections();
            DataContext = App.ConnectionsVM;
        }

        private async void ConnnectToRecentIp()
        {
            var ip = (string)SettingsHelper.GetValue("RecentServerIP");
            if (ip != null)
            {
                var connectionItem = App.ConnectionsVM.ConnectionItems.FirstOrDefault(item => item.IpAddress == ip);
                if (connectionItem != null)
                    await ConnectToServer(connectionItem);
            }
        }

        private async Task ConnectToServer(ConnectionItem connectionItem)
        {
            SetPageState(PageStates.Connecting);
            bool isSuccessful = false;
            try
            {
                isSuccessful = await JSONRPC.Ping(connectionItem);
            }
            catch
            {
                isSuccessful = false;
            }
            if (isSuccessful)
            {
                ConnectionManager.CurrentConnection = connectionItem;
                SettingsHelper.SetValue("RecentServerIP", connectionItem.IpAddress);
                Frame.Navigate(typeof(CoverPage));
            }
            else
            {
                MessageDialog message = new MessageDialog(loader.GetString("ConnectionUnsuccessful_Content"), loader.GetString("ConnectionUnsuccessful_Title"));
                await message.ShowAsync();
                SetPageState(PageStates.Ready);
            }
        }

        private void SetPageState(PageStates pageState)
        {
            if (pageState == PageStates.Connecting)
            {
                ConnectionsListView.IsEnabled = false;
                BottomAppBar.Visibility = Visibility.Collapsed;
                ProgressRing.IsActive = true;
            }
            else
            {
                ConnectionsListView.IsEnabled = true;
                BottomAppBar.Visibility = Visibility.Visible;
                ProgressRing.IsActive = false;
            }
        }

        private async void ConnectionItemWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectionItem selectedConnection = (ConnectionItem)(sender as StackPanel).DataContext;
            await ConnectToServer(selectedConnection);
        }

        private void AddConnectionAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddConnectionPage));
        }

        private async void DeleteConnectionMFI_Click(object sender, RoutedEventArgs e)
        {
            ConnectionItem selectedConnection = (ConnectionItem)(sender as MenuFlyoutItem).DataContext;
            await App.ConnectionsVM.RemoveConnectionItem(selectedConnection).ConfigureAwait(true);
        }

        private void EditConnectionMFI_Click(object sender, RoutedEventArgs e)
        {
            ConnectionItem selectedConnection = (ConnectionItem)(sender as MenuFlyoutItem).DataContext;
            Frame.Navigate(typeof(EditConnectionPage), selectedConnection);
        }


        private void AboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPivot));
        }

        private void ConnectionItemWrapper_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ConnectionsListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectionsListView.SelectedItem = null;
        }
    }
}
