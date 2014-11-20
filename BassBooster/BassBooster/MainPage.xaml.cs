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
        private bool _Empty; // true if there aren't any tracks loaded
        private bool _Shuffle;
        private int _CurrentId;
        private List<Windows.Storage.StorageFile> _Playlist = null;
        private List<int> _ShufflePlaylist;
        private int _ShuffleId;
        private TrackList _Tracklist;
        private DispatcherTimer _Timer = new DispatcherTimer() ;//timer for slider
        private Repeat _Repeat;
        public static bool LoggedIn = false;
        
        

        #endregion

        #region Constructors
        public MainPage()
        {
            this.InitializeComponent();            
            _Tracklist = new TrackList();
            MP3Player.MediaEnded += MP3Player_MediaEnded;
            MP3Player.MediaOpened += MP3Player_MediaOpened;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            _Timer.Tick += Tick_Action;
            _Repeat = Repeat.ALL;
            _ShufflePlaylist = new List<int>();
            MP3Player.Volume = 1.0;
            TrackListBox.ItemsSource = null;
            _Empty = true;
            _Shuffle = false;            
        }
        #endregion

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
                if (!_Empty)
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
            _Timer.Stop();
            NextButton_Click(null, null);            
        }

        private void MP3Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Value = 0;
            TimeSlider.Maximum = MP3Player.NaturalDuration.TimeSpan.TotalMilliseconds;
            _Timer.Interval = TimeSpan.FromMilliseconds(1);            
            _Timer.Start();
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
            else if(!_Empty && (MP3Player.CurrentState == MediaElementState.Paused || MP3Player.CurrentState == MediaElementState.Stopped ))
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
            if (_Playlist == null)
                _Playlist = new List<Windows.Storage.StorageFile>();

            IReadOnlyList<Windows.Storage.StorageFile> files = await fop.PickMultipleFilesAsync();
            //check if list of selected files isn't empty 
            if (files.Count > 0)
            {
                //object for music properties
                MusicProperties musicProperties;               
                int i = 0;
                foreach (var f in files)
                {
                    _Playlist.Add(f);
                    musicProperties = await f.Properties.GetMusicPropertiesAsync();
                    _Tracklist.Add(new Track (_Tracklist.Music.Count,musicProperties.Artist,musicProperties.Title,f.Name,musicProperties.Duration));
                    i++;
                }
                //setting stream       
                var stream = await _Playlist[0].OpenAsync(Windows.Storage.FileAccessMode.Read);
                if (!wasPlaying)
                {
                    MP3Player.SetSource(stream, _Playlist[0].ContentType);
                    //mark currently played file
                    _CurrentId = 0;
                    int time = _Tracklist.GetDurationIntById(_CurrentId);
                    TitleBox.Text = _Tracklist.TrackToString(0);
                    TileManager.UpdateTile(TitleBox.Text,time);
                    ToastManager.ShowToast(TitleBox.Text);
                }
                if (_Shuffle)
                {
                    Shuffle();
                }
                //refresh track listbox
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = _Tracklist.Music;
                _Empty = false;
                MP3Player.Play();
                TrackListBox.SelectedIndex = _CurrentId;
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
            if (_Repeat == Repeat.ALL)
            {
                if (_Empty == false)
                {
                    MP3Player.Stop();
                    if (!_Shuffle)
                        if (_CurrentId < (_Tracklist.Music.Count - 1))
                            _CurrentId++;
                        else
                            _CurrentId = 0;
                    else
                    {
                        _ShuffleId++;
                        if (_ShuffleId >= _Playlist.Count)
                            _ShuffleId = 0;
                        _CurrentId = _ShufflePlaylist[_ShuffleId];
                    }
                    CommonAction();
                }
            }
            else
                CommonAction();

        }


        //previous
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_Empty == false)
            {
                if (_Repeat == Repeat.ALL)
                {
                    MP3Player.Stop();
                    if (!_Shuffle)
                        if (_CurrentId > 0)
                            _CurrentId--;
                        else
                            _CurrentId = _Tracklist.Music.Count - 1;
                    else
                    {
                        _ShuffleId--;
                        if (_ShuffleId < 0)
                            _ShuffleId = _Playlist.Count - 1;
                        _CurrentId = _ShufflePlaylist[_ShuffleId];
                    }
                }
                CommonAction();
            }
        }

        //clean
        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_Empty)
            {
                MP3Player.Stop();
                _Tracklist = new TrackList();
                _Empty = true;
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = _Tracklist.Music;
                TitleBox.Text = "Artist - Title";
                _Playlist.Clear();
                TileManager.ClearTile();
            }

            

        }

        //shuffle on/off
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_Shuffle)
            {
                _Shuffle = false;
                ShuffleButton.Label = "Shuffle is off";
            }
            else
            {
                
                _Shuffle = true;
                ShuffleButton.Label = "Shuffle is on";
                Shuffle();
                if (MP3Player.CurrentState == MediaElementState.Playing)
                {
                    _ShuffleId = _ShufflePlaylist.IndexOf(_CurrentId);                                 
                }
                else
                {
                    _ShuffleId = 0;
                }
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (_Repeat == Repeat.ALL)
            {
                _Repeat = Repeat.ONE;
                RepeatButton.Icon = new SymbolIcon(Symbol.RepeatOne);
                RepeatButton.Label = "Repeat One";
            }
            else
            {
                _Repeat = Repeat.ALL;
                RepeatButton.Icon = new SymbolIcon(Symbol.RepeatAll);
                RepeatButton.Label = "Repeat All";
            }
        }
        

          
        // tracklist selection f        
        private void TrackListBox_SelectionChanged(object sender, DoubleTappedRoutedEventArgs e)
        {
            MP3Player.Stop();
            _CurrentId = TrackListBox.SelectedIndex;
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
            int time = _Tracklist.GetDurationIntById(_CurrentId);
            //Timer = new DispatcherTimer();
            var stream = await _Playlist[_CurrentId].OpenAsync(Windows.Storage.FileAccessMode.Read);
            MP3Player.SetSource(stream, _Playlist[_CurrentId].ContentType);
            MP3Player.Play();
            TrackListBox.SelectedIndex = _CurrentId;
            TitleBox.Text = _Tracklist.TrackToString(_CurrentId);
            TileManager.UpdateTile(TitleBox.Text,time);
            ToastManager.ShowToast(TitleBox.Text);
        }


        //Updating tile
        
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
            if (seconds < 10 )
                return (minute + " : 0" + seconds);
            else
                return (minute + " : " + seconds);
        }

        private void NotifAppButton_Clicked(object sender, RoutedEventArgs e)
        {

            if (ToastManager.isEnabled)
            {
                ToastManager.isEnabled = false;
                NotifAppButton.Label = "Toast Off";
            }
            else
            {
                ToastManager.isEnabled = true;
                NotifAppButton.Label = "Toast On";
            }
        }

        private void TileAppButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TileManager.isEnabled)
                {
                    TileAppButton.Label = "Dynamic tile off";
                    TileManager.isEnabled = false;                    
                }
                else
                {
                    TileManager.isEnabled = true;
                    TileAppButton.Label = "Dynamic tile on";
                }
            }
            catch (NullReferenceException exc)
            {
                TileManager.isEnabled = true;
            }
        }

        public void Shuffle()
        {
            _ShufflePlaylist = new List<int>();
            int i = 0;
            
            foreach (var song in _Playlist){
                _ShufflePlaylist.Add(i++);
            }

            Random rnd = new Random();
            _ShufflePlaylist = (from t in _ShufflePlaylist
                                select t).OrderBy(item => rnd.Next()).ToList();
        }

        #endregion

        

        

    }
    enum Repeat
    {
        ALL, ONE
    }
}
