using BassBooster.Common;
using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

//TODO: Tile, repeat, time slider

namespace BassBooster
{
    /// <summary>
    /// MediaElement doesn't support FLAC
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Vars
        public static string CurrentlyPlaying;
        private bool Empty; // true if there aren't any tracks loaded
        private bool Shuffle;
        private int CurrentList;
        private int CurrentTrack;
        private int CurrentId;
        private List<IReadOnlyList<Windows.Storage.StorageFile>> Playlist = null; // adding files as 'miniplaylist' every time we add new FileOpenPicker because object that we get as playlist is readonly
        private TrackList Tracklist;

        #endregion

        public MainPage()
        {
            this.InitializeComponent();            
            Tracklist = new TrackList();
            MP3Player.MediaEnded += MP3Player_MediaEnded;
            MediaControl.PausePressed += MediaControl_pp;
            MediaControl.PlayPressed += MediaControl_plp;
            MediaControl.PlayPauseTogglePressed += MediaControl_plptp;
            MediaControl.StopPressed += MediaControl_sp;
            MediaControl.NextTrackPressed += MediaControl_ntp;
            MediaControl.PreviousTrackPressed += MediaControl_ptp;            
            MP3Player.Volume = 1.0;
            TrackListBox.ItemsSource = null;
            Empty = true;
            Shuffle = false;
            
        }

        

             

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //setting current tab as selected
            NavTab.ItemsSource = TabList;
            
            //check if SM has saved tab
            if (SuspensionManager.SessionState.ContainsKey("CurrentTab"))
            {
                NavTab.SelectedIndex = Convert.ToInt32(SuspensionManager.SessionState["CurrentTab"]);
                NavTab.ScrollIntoView(NavTab.SelectedItem);
            }
            else
            {
                NavTab.SelectedIndex = 0;
            }
        }

        private void NavTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                   
            ListBox tabList = sender as ListBox;
            Tab t = tabList.SelectedItem as Tab;
            //save state to SM
            if (t != null)
            {
                if (t.ClassType.Equals(typeof(ListPage)))
                    TrackListBox.Visibility = Visibility.Visible;
                else
                    TrackListBox.Visibility = Visibility.Collapsed;
                SuspensionManager.SessionState["CurrentTab"] = tabList.SelectedIndex;
                TabFrame.Navigate(t.ClassType);
            }
        }

        #region PlayerManagement

        //using dispatcher to get access to thread that runs player
        //next track pressed
        private async void MediaControl_ntp(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NextButton_Click(null, null);
            });
        }
        //previous track pressed
        private async void MediaControl_ptp(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PrevButton_Click(null, null);
            });
        }   

        //stopped pressed
        private async void MediaControl_sp(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MP3Player.Stop();
            });
        }

        // action for play pause button when minimalized
        private async void MediaControl_plptp(object sender, object e)
        {
            //if app minimalized getting access to bgtask and pausing/playing 
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (this.MP3Player.CurrentState == MediaElementState.Playing)
                {
                    this.MP3Player.Pause();
                }
                else
                {
                    this.MP3Player.Play();
                }
            });
        }

        //play  pressed
        private async void MediaControl_plp(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!Empty)
                    MP3Player.Play();
            });
        }

        //pause pressed
        private async void MediaControl_pp(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MP3Player.Pause();
            });
        }

        private void MP3Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            NextButton_Click(null, null);
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MP3Player != null)
                MP3Player.Volume = ((double)VolumeSlider.Value) / 100.0;
        }
        
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MP3Player.CurrentState == MediaElementState.Playing){
                MP3Player.Pause();
                PlayButton.Icon = new SymbolIcon(Symbol.Play);
                
            }
            else if(!Empty && (MP3Player.CurrentState == MediaElementState.Paused || MP3Player.CurrentState == MediaElementState.Stopped ))
            {
                MP3Player.Play();
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            }
        }

        
        private async void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            bool wasPlaying;
            if (MP3Player.CurrentState == MediaElementState.Playing)
            {
                MP3Player.Pause();
                wasPlaying = true;
            }
            else
                wasPlaying = false;

            Windows.Storage.Pickers.FileOpenPicker fop = new Windows.Storage.Pickers.FileOpenPicker();
            //adding file types
            fop.FileTypeFilter.Add(".mp3");
            fop.FileTypeFilter.Add(".wav");
            fop.FileTypeFilter.Add(".wma");
            fop.FileTypeFilter.Add(".aac");
            fop.FileTypeFilter.Add(".asf");
            //if this.plyalist is empty create new list
            if (Playlist == null)
                Playlist = new List<IReadOnlyList<Windows.Storage.StorageFile>>();

            IReadOnlyList<Windows.Storage.StorageFile> files = await fop.PickMultipleFilesAsync();
            //check if list of selected files isn't empty 
            if (files.Count > 0)
            {
                //object for music properties
                MusicProperties musicProperties;
                //adding 'miniplaylist' to playlist
                Playlist.Add(files);                
                int currentPlIndex = Playlist.Count - 1; // getting number of already existing 'miniplaylisy'
                //add 'miniplaylist' ranges
                Tracklist.RangeAdd(currentPlIndex, files.Count);
                int i = 0;
                int sec;
                foreach (var f in files)
                {
                    musicProperties = await f.Properties.GetMusicPropertiesAsync();
                    Tracklist.Add(new Track (i,currentPlIndex,Tracklist.Music.Count,musicProperties.Artist,musicProperties.Title,f.Name,musicProperties.Duration));
                    i++;
                }
                //setting stream       
                var stream = await files[0].OpenAsync(Windows.Storage.FileAccessMode.Read);
                if (!wasPlaying)
                {
                    MP3Player.SetSource(stream, Playlist[currentPlIndex][0].ContentType);
                    //mark currently played file
                    CurrentList = currentPlIndex;
                    CurrentTrack = 0;
                    CurrentId = 0;
                    TitleBox.Text = Tracklist.TrackToString(0);
                    TimeBox.Text = Tracklist.GetDurationStringById(0);
                    UpdateTile(Tracklist.GetDurationIntById(0));
                }
                //refresh track listbox
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = Tracklist.Music;
                Empty = false;
                MP3Player.Play();
                TrackListBox.SelectedIndex = CurrentId;
                //set button icon to pause
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);

            }
            else //if no file was chosen then continue
            {
                if (wasPlaying)
                    MP3Player.Play();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Empty == false)
            {
                MP3Player.Stop();
                if (!Shuffle)
                    if (CurrentId < (Tracklist.Music.Count - 1))
                        CurrentId++;
                    else
                        CurrentId = 0;
                else
                    CurrentId = (new Random()).Next(0, (int)Tracklist.Length);
                CommonAction();
            }

        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (Empty == false)
            {
                MP3Player.Stop();
                if (!Shuffle)
                    if (CurrentId > 0)
                        CurrentId--;
                    else
                        CurrentId = Tracklist.Music.Count - 1;
                else
                    CurrentId = (new Random()).Next(0, (int)Tracklist.Length);
                CommonAction();
            }
        }

        private async void CommonAction()
        {
            //gathering file postion on playlsit
            int[] xx = Tracklist.FindById(CurrentId);
            CurrentTrack = xx[0];
            CurrentList = xx[1];
            var stream = await Playlist[CurrentList][CurrentTrack].OpenAsync(Windows.Storage.FileAccessMode.Read);
            MP3Player.SetSource(stream, Playlist[CurrentList][CurrentTrack].ContentType);
            MP3Player.Play();
            //updating page
            TrackListBox.SelectedIndex = CurrentId;
            TitleBox.Text = Tracklist.TrackToString(CurrentId);
            TimeBox.Text = Tracklist.GetDurationStringById(CurrentId);
            UpdateTile(Tracklist.GetDurationIntById(CurrentId));
        }

        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Empty) { 
                MP3Player.Stop();
                Tracklist = new TrackList();
                Empty = true;            
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = Tracklist.Music;
                Playlist.Clear();
                Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            }
        }
        
        private void UpdateTile(int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = TitleBox.Text;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);

        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (Shuffle)
            {
                Shuffle = false;
                ShuffleButton.Label = "Shuffle is off";
            }
            else
            {
                Shuffle = true;
                ShuffleButton.Label = "Shuffle is on";
            }
        }

        private void TrackListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MP3Player.Stop();
            CurrentId = TrackListBox.SelectedIndex;
            CommonAction();
        }

        private void TrackListBox_SelectionChanged(object sender, DoubleTappedRoutedEventArgs e)
        {
            MP3Player.Stop();
            CurrentId = TrackListBox.SelectedIndex;
            CommonAction();
        }

        #endregion

        #region Tabs


        public List<Tab> TabList = new List<Tab>
        {
            new Tab() { Name = "   ⏯  Playlist", ClassType = typeof(ListPage) },
            new Tab() { Name = "   ⌨ Lyrics", ClassType = typeof(LyricsPage) },
            new Tab() { Name = "   ③  OneDrive - synchronizing lyrics", ClassType = typeof(OneDriveSyncPage) }
        };


        #endregion

        

        
    }
    enum Repeat
    {
        NONE, ALL, ONE
    }
}
