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
using Microsoft.Live;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

//TODO:  repeat

namespace BassBooster
{
    /// <summary>
    /// MediaElement doesn't support FLAC
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Vars
        private bool Empty; // true if there aren't any tracks loaded
        private bool Shuffle;
        private int CurrentList;
        private int CurrentTrack;
        private int CurrentId;
        private List<IReadOnlyList<Windows.Storage.StorageFile>> Playlist = null; // adding files as 'miniplaylist' every time we add new FileOpenPicker because object that we get as playlist is readonly
        private TrackList Tracklist;
        private DispatcherTimer Timer = new DispatcherTimer() ;//timer for slider
        public static bool LoggedIn = false;
        

        #endregion

        public MainPage()
        {
            this.InitializeComponent();            
            Tracklist = new TrackList();
            MP3Player.MediaEnded += MP3Player_MediaEnded;
            MP3Player.MediaOpened += MP3Player_MediaOpened;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            Timer.Tick += Tick_Action;
            MP3Player.Volume = 1.0;
            TrackListBox.ItemsSource = null;
            Empty = true;
            Shuffle = false;            
        }

        
        #region Navigation
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

        #endregion

        #region PlayerManagement

        #region MediaControl
        //using dispatcher to get access to thread that runs player
        //next track pressed
        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NextButton_Click(null, null);
            });
        }
        //previous track pressed
        private async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PrevButton_Click(null, null);
            });
        }   

        //stopped pressed
        private async void MediaControl_StopPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MP3Player.Stop();
            });
        }

        // action for play pause button when minimalized
        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
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
        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!Empty)
                    MP3Player.Play();
            });
        }

        //pause pressed
        private async void MediaControl_PausePressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MP3Player.Pause();
            });
        }
        #endregion

        #region MP3PlayerEvents
        private void MP3Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            NextButton_Click(null, null);            
        }

        private void MP3Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Value = 0;
            TimeSlider.Maximum = MP3Player.NaturalDuration.TimeSpan.TotalMilliseconds;
            Timer.Interval = TimeSpan.FromMilliseconds(1);            
            Timer.Start();
        }

        #endregion

        #region ButtonAction


        //volume change
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MP3Player != null)
                MP3Player.Volume = ((double)VolumeSlider.Value) / 100.0;
        }
        
        //play Pause
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

        //open files
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
                int i = 0;
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
                    int time = Tracklist.GetDurationIntById(CurrentId);
                    TitleBox.Text = Tracklist.TrackToString(0);
                    UpdateTile(time);
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

        //next
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


        //previous
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

        //clean
        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Empty)
            {
                MP3Player.Stop();
                Tracklist = new TrackList();
                Empty = true;
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = Tracklist.Music;
                TitleBox.Text = "Artist - Title";
                Playlist.Clear();
                Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            }
        }

        //shuffle on/off
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
        

          
        // tracklist selection f        
        private void TrackListBox_SelectionChanged(object sender, DoubleTappedRoutedEventArgs e)
        {
            MP3Player.Stop();
            CurrentId = TrackListBox.SelectedIndex;
            CommonAction();
        }

        //slider change
        private void TimeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)TimeSlider.Value);
            MP3Player.Position = ts;
        }
        #endregion

        #region SharedOtherAction
        //action per timer tick 
        private void Tick_Action(object sender, object e)
        {
            TimeBox.Text = MillisecondsToMinute((long)MP3Player.Position.TotalMilliseconds);
            TimeSlider.Value = MP3Player.Position.TotalMilliseconds;
        }


        //shared action for song change
        private async void CommonAction()
        {
            //gathering file postion on playlsit
            int[] xx = Tracklist.FindById(CurrentId);
            CurrentTrack = xx[0];
            CurrentList = xx[1];
            int time = Tracklist.GetDurationIntById(CurrentId);
            //Timer = new DispatcherTimer();
            var stream = await Playlist[CurrentList][CurrentTrack].OpenAsync(Windows.Storage.FileAccessMode.Read);
            MP3Player.SetSource(stream, Playlist[CurrentList][CurrentTrack].ContentType);
            MP3Player.Play();
            //updating page
            TrackListBox.SelectedIndex = CurrentId;
            TitleBox.Text = Tracklist.TrackToString(CurrentId);
            UpdateTile(time);
        }


        //Updating tile
        private void UpdateTile(int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = TitleBox.Text;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
        #endregion


        #endregion

        #region Tabs


        public List<Tab> TabList = new List<Tab>
        {
            new Tab() { Name = "   ⏯  Playlist", ClassType = typeof(ListPage) },
            new Tab() { Name = "   ⌨ Lyrics", ClassType = typeof(LyricsPage) },
            new Tab() { Name = "   ③  OneDrive - synchronizing lyrics", ClassType = typeof(OneDriveSyncPage) }
        };


        #endregion

        #region Others
        public string MillisecondsToMinute(long milliseconds)
        {
            int minute = (int)(milliseconds / (1000 * 60));
            int seconds = (int)((milliseconds /  1000) % 60 );
            if (seconds <10 )
                return (minute + " : 0" + seconds);
            else
                return (minute + " : " + seconds);
        }

        #endregion
    }
    enum Repeat
    {
        NONE, ALL, ONE
    }
}
