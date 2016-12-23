﻿using XBMCRemoteRT.Common;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224
using XBMCRemoteRT.Helpers;
using XBMCRemoteRT.Models;
using XBMCRemoteRT.Models.Audio;
using XBMCRemoteRT.Models.Common;
using XBMCRemoteRT.Models.Video;
using XBMCRemoteRT.Pages;
using XBMCRemoteRT.Pages.Audio;
using XBMCRemoteRT.Pages.Files;
using XBMCRemoteRT.Pages.Video;
using XBMCRemoteRT.RPCWrappers;

namespace XBMCRemoteRT
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class CoverPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private DispatcherTimer timer;
        
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

        public CoverPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

            if (GlobalVariables.CurrentPlayerState == null) {
                GlobalVariables.CurrentPlayerState = new PlayerState();
            }
            DataContext = GlobalVariables.CurrentPlayerState;

            PlayerHelper.RefreshPlayerState().Wait(200);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += timer_Tick;
        }

        async void timer_Tick(object sender, object o)
        {
            await PlayerHelper.RefreshPlayerState();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            RefreshListsIfNull();
            ServerNameTextBlock.Text = ConnectionManager.CurrentConnection.ConnectionName;
            Frame.BackStack.Clear();
            TileHelper.UpdateAllTiles();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private List<Album> Albums;
        private List<Episode> Episodes;
        private List<Movie> Movies;

        
        private async void RefreshListsIfNull()
        {
            if (Albums == null)
            {
                Albums = await AudioLibrary.GetRecentlyAddedAlbums(new Limits { Start = 0, End = 8 });
                MusicHubSection.DataContext = Albums;
            }

            if (Episodes == null)
            {
                Episodes = await VideoLibrary.GetRecentlyAddedEpisodes(new Limits { Start = 0, End = 8 });
                TVHubSection.DataContext = Episodes;
            }

            if (Movies == null)
            {
                Movies = await VideoLibrary.GetRecentlyAddedMovies(new Limits { Start = 0, End = 8 });
                MoviesHubSection.DataContext = Movies;
            }
        }

        private void RemoteAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(InputPage));
        }

        private void FilesAppBarButton_OnClick(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(AllSourcesPage));
        }

        private void MusicHeaderWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AllMusicPage));
        }

        private void TVShowsHeaderWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AllTVShowsPage));
        }

        private void MoviesHeaderWrapper_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AllMoviesPage));
        }

        private async void CycleRepeatButton_Tapped(object sender, TappedRoutedEventArgs e) {
            string nextRepeat = "off";
            switch (GlobalVariables.CurrentPlayerState.Repeat) {
                case "off":
                    nextRepeat = "all";
                    break;
                case "all":
                    nextRepeat = "one";
                    break;
            }
            await Player.SetRepeat(GlobalVariables.CurrentPlayerState.PlayerType, nextRepeat);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void ToggleShuffleButton_Tapped(object sender, TappedRoutedEventArgs e) {
            await Player.SetShuffle(GlobalVariables.CurrentPlayerState.PlayerType, !GlobalVariables.CurrentPlayerState.Shuffle);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.GoTo(GlobalVariables.CurrentPlayerState.PlayerType, GoTo.Previous);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.PlayPause(GlobalVariables.CurrentPlayerState.PlayerType);
            await PlayerHelper.RefreshPlayerState();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await Player.GoTo(GlobalVariables.CurrentPlayerState.PlayerType, GoTo.Next);
            await PlayerHelper.RefreshPlayerState();
        }

        private void ConnectionsAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), false);
        }

        private void AboutAppBarButton_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(AboutPivot));
        }

        private void AlbumGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tappedAlbum = e.ClickedItem as Album;
            GlobalVariables.CurrentAlbum = tappedAlbum;
            Frame.Navigate(typeof(AlbumPage));
        }

        private void EpisodeGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tappedEpisode = e.ClickedItem as Episode;
            Player.PlayEpisode(tappedEpisode);
        }

        private void MovieGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tappedMovie = e.ClickedItem as Movie;
            GlobalVariables.CurrentMovie = tappedMovie;
            Frame.Navigate(typeof(MovieDetailsHub));
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (NowPlaying));
        }

        private void MusicOpticalDiscWrapper_Tapped(object sender, TappedRoutedEventArgs e) {
            Player.PlayDirectory("cdda://local/");
        }

        private async void EjectButton_Click(object sender, RoutedEventArgs e) {
            await Input.ExecuteAction(SystemCommands.EjectOpticalDrive);
        }

        Slider slider;
        private void ProgressSlider_Loaded(object sender, RoutedEventArgs e) {
            slider = sender as Slider;
            slider.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(slider_PointerReleased), true);
        }

        void slider_PointerReleased(object sender, PointerRoutedEventArgs e) {
            var percentage = (slider.Value * 100) / slider.Maximum;
            Player.Seek(GlobalVariables.CurrentPlayerState.PlayerType, percentage);
        }
    }
}
