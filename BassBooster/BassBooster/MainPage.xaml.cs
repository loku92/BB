using BassBooster.Common;
using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238


namespace BassBooster
{
    /// <summary>
    /// Contains MP3 Player Code
    /// MediaElement doesn't support FLAC.
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
        private bool _IsKeyboardActive;
        
        

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
            _Empty = true;
            _Shuffle = false;            
            MP3Player.Volume = 1.0;
            TrackListBox.ItemsSource = null;
            _IsKeyboardActive = true;
        }
        #endregion

        #region Navigation

        /// <summary>
        /// OnNavigate  - tab changing code
        /// </summary>
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

            //should be in onLaunched but sometimes it doesnt work there
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("BgId"))
            {
                BgId = (int)(ApplicationData.Current.LocalSettings.Values["BgId"]);
            }

            SetImage();
        }

        /// <summary>
        /// NavTav_SelectionChanged - when we click on tab what tab to be displayed
        /// </summary>

        private void NavTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {                   
            ListBox tabList = sender as ListBox;
            Tab t = tabList.SelectedItem as Tab;
            //save state to SM
            if (t != null)
            {
                if (t.ClassType.Equals(typeof(ListPage)))
                {
                    _IsKeyboardActive = true;
                    TrackListBox.Visibility = Visibility.Visible;
                    BgImage.Visibility = Visibility.Visible;
                }
                else
                {
                    _IsKeyboardActive = false;
                    TrackListBox.Visibility = Visibility.Collapsed;
                    BgImage.Visibility = Visibility.Collapsed;
                }
                SuspensionManager.SessionState["CurrentTab"] = tabList.SelectedIndex;
                TabFrame.Navigate(t.ClassType);
            }
        }

        #endregion

        #region PlayerManagement


        /// <summary>
        /// MediaContol
        /// For controling MP3 Player
        /// </summary>
        #region MediaControl


        /// <summary>
        /// MediaControl_NextTrackPressed
        /// When app is minimalized and next button is pressed
        /// </summary>
        
        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Next();
            });
        }


        /// <summary>
        /// MediaControl_PreviousTrackPressed
        ///When app is minimalized and prev button is pressed
        /// </summary>
        private async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Prev();
            });
        }

        /// <summary>
        /// MediaControl_StopPressed
        /// When app is minimalized and stop button is pressed
        /// </summary>
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

        /// <summary>
        /// MP3Player_MediaEnded
        /// What to do when we track has finished
        /// </summary>
        private void MP3Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            _Timer.Stop();
            Next();            
        }


        /// <summary>
        /// MP3Player_MediaOpened
        /// What to do when we have loaded song successfully
        /// </summary>
        private void MP3Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Value = 0;
            TimeSlider.Maximum = MP3Player.NaturalDuration.TimeSpan.TotalMilliseconds;
            _Timer.Interval = TimeSpan.FromMilliseconds(1);            
            _Timer.Start();
        }

        /// <summary>
        /// Select next song and play
        /// </summary>
        private void Next()
        {
            if (_Empty == false)
            {
                //check if isn't empty
                if (_Repeat == Repeat.ALL)
                {
                    MP3Player.Stop();
                    //check if shuffle is on
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
                        try
                        {
                            _CurrentId = _ShufflePlaylist[_ShuffleId];
                        }
                        catch (ArgumentOutOfRangeException exception) //occures when 1st tracklist is loaded from files and shuffle is pressed
                        {
                            Shuffle();
                            _CurrentId = _ShufflePlaylist[_ShuffleId];
                        }
                    }
                    CommonAction();
                }
                else
                    CommonAction();
            }
        }

        /// <summary>
        /// Select previous song and play
        /// </summary>
        private void Prev()
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

        #endregion

        #region ButtonAction


        /// <summary>
        /// VolumeSlider_ValueChanged
        /// Volume slider code
        /// </summary>
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

        /// <summary>
        /// FileOpenButton_Click
        /// Opens FileOpenPicker to load tracks
        /// </summary>
        private async void FileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            //check if was playing and flag it
            bool wasPlaying;
            if (MP3Player.CurrentState == MediaElementState.Playing)
            {
                MP3Player.Pause();
                wasPlaying = true;
            }
            else
                wasPlaying = false;
            //create file open picker to select new files to open
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
            //open fop window
            IReadOnlyList<Windows.Storage.StorageFile> files = await fop.PickMultipleFilesAsync();
            //check if list of selected files isn't empty 
            if (files.Count > 0)
            {
                //object for music properties that contains artist,title,duration etc
                MusicProperties musicProperties;               
                int i = 0;
                //foreach chosen file add it to _Playlist(files) and _Tracklist(list of titles)
                foreach (var f in files)
                {
                    _Playlist.Add(f);
                    musicProperties = await f.Properties.GetMusicPropertiesAsync();
                    _Tracklist.Add(new Track (_Tracklist.Music.Count,musicProperties.Artist,musicProperties.Title,f.Name,musicProperties.Duration));                    
                    i++;
                }
                //refresh track listbox 
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = _Tracklist.Music;
                // if wasn't playing set new source for MediaElement and update some UI elements
                if (!wasPlaying)
                {
                    var stream = await _Playlist[0].OpenAsync(Windows.Storage.FileAccessMode.Read);
                    MP3Player.SetSource(stream, _Playlist[0].ContentType);
                    //mark currently played file
                    _CurrentId = 0;
                    int time = _Tracklist.GetDurationIntById(_CurrentId);
                    TitleBox.Text = _Tracklist.TrackToString(0);
                    TileManager.UpdateTile(TitleBox.Text,time);
                    ToastManager.ShowToast(TitleBox.Text);
                }
                //if shuffle was on, shuffle list
                if (_Shuffle)
                {
                    Shuffle();
                }
                //select currently played song on list
                TrackListBox.SelectedIndex = _CurrentId;                
                _Empty = false;
                MP3Player.Play();
                //set button icon to pause
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);

            }
            else //if any file was chosen then continue
            {
                if (wasPlaying)
                    MP3Player.Play();
            }
        }

        //next
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Next(); 
        }



        //previous
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            Prev();
        }


        /// <summary>
        /// ClearListButton_Click
        /// To clear playlist
        /// </summary>
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
                PlayButton.Icon = new SymbolIcon(Symbol.Play);
            }
        }

        /// <summary>
        /// ShuffleButton_Click
        /// Turn on/off playlist shuffle
        /// </summary>
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


        /// <summary>
        /// RepeatButton_Click
        /// Switch repeat whole tracklist or just 1 song
        /// </summary>
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

        /// <summary>
        /// TrackListBox_SelectionChanged
        /// Executed when we double click on playlist track
        /// </summary>     
        private void TrackListBox_SelectionChanged(object sender, DoubleTappedRoutedEventArgs e)
        {
            MP3Player.Stop();
            _CurrentId = TrackListBox.SelectedIndex;
            CommonAction();
        }

        /// <summary>
        /// TimeSlider_ValueChanged
        /// Executed when we change position of time slider to navigate a song to right place
        /// </summary> 
        private void TimeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)TimeSlider.Value);
            MP3Player.Position = ts;
        }
        #endregion


        /// <summary>
        /// Shared action
        ///Repeated or some other code in one place
        /// </summary> 
        #region SharedOtherAction


        /// <summary>
        /// Tick_Action
        /// Tick on timer to move TimeSlider
        /// </summary> 
        private void Tick_Action(object sender, object e)
        {
            TimeBox.Text = MillisecondsToMinute((long)MP3Player.Position.TotalMilliseconds);
            TimeSlider.Value = MP3Player.Position.TotalMilliseconds;
        }


        /// <summary>
        /// CommonAction
        /// Called when song is changed, updates UI and starts playing.
        /// </summary>
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

        /// <summary>
        /// List of tabs (frames) to be displayed in list (on the left side of UI)
        /// </summary>
        public List<Tab> TabList = new List<Tab>
        {
            new Tab() { Name = "   ⏯  Playlist", ClassType = typeof(ListPage) },
            new Tab() { Name = "   ⌨ Lyrics", ClassType = typeof(LyricsPage) },
            new Tab() { Name = "   ③  OneDrive - synchronizing lyrics", ClassType = typeof(OneDriveSyncPage) }
        };


        #endregion

        #region Others

        /// <summary>
        /// MillisecondsToMinute
        ///     COnverts time of song from miliseconds to displayable string minutes:seconds
        /// </summary>
        public string MillisecondsToMinute(long milliseconds)
        {
            int minute = (int)(milliseconds / (1000 * 60));
            int seconds = (int)((milliseconds /  1000) % 60 );
            if (seconds < 10 )
                return (minute + " : 0" + seconds);
            else
                return (minute + " : " + seconds);
        }

        /// <summary>
        /// NotifAppButton_Clicked
        ///     Turn on/off toasts
        /// </summary>
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

        /// <summary>
        /// TileAppButton_Clicked
        ///     Turn on/off dynamic tile
        /// </summary>
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

        private async void AuthorAppButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dlg = new MessageDialog("Author: Daniel Pawłowski \nE-mail: pawlowsd@ee.pw.edu.pl \nVersion: 1.0", "About");
            await dlg.ShowAsync();
        }

        /// <summary>
        /// Shuffle
        ///     Shuffle our playlist.
        /// </summary>
        public void Shuffle()
        {
            if (!_Empty)
            {
                _ShufflePlaylist = new List<int>();
                int i = 0;

                foreach (var song in _Playlist)
                {
                    _ShufflePlaylist.Add(i++);
                }

                Random rnd = new Random();
                _ShufflePlaylist = (from t in _ShufflePlaylist
                                    select t).OrderBy(item => rnd.Next()).ToList();
            }
        }

        #endregion

        #region Background Image
        /// <summary>
        /// BgAppButton_Click action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgAppButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (BgId == 3)
            {
                BgId = 0;
            }
            else
            {
                BgId++;
            }
            ApplicationData.Current.LocalSettings.Values["BgId"] = BgId;
            SetImage();
        }

        /// <summary>
        /// Changes bg image
        /// </summary>
        private void SetImage()
        {
            string[] bg = { "Assets/MainPage/background-218180.jpg", "Assets/MainPage/green-19916.jpg", "Assets/MainPage/pink-19750.jpg", "Assets/MainPage/pink-240518.jpg" };
            BitmapImage newBg = new BitmapImage(new Uri(this.BaseUri, bg[BgId]));
            BgImage.Source = newBg;
        }

        public static int BgId = 0;
        #endregion

        #region KeyListener

        /// <summary>
        /// Sets KeyUp event listening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyUp_Event(object sender, KeyRoutedEventArgs e)
        {
            if (_IsKeyboardActive)
            {
                switch (e.Key)
                {
                    case VirtualKey.Enter:
                        _CurrentId = TrackListBox.SelectedIndex;
                        CommonAction();
                        break;
                    case VirtualKey.N:
                        Next();
                        break;
                    case VirtualKey.B:
                        Prev();
                        break;
                    case VirtualKey.Space:
                        PlayButton_Click(null, null);
                        break;
                    case VirtualKey.S:
                        ShuffleButton_Click(null, null);
                        break;
                }
            }
        }
        #endregion

    }


    /// <summary>
    /// Repeat
    ///     ONE - just one track
    ///     ALL - whole playlist
    /// </summary>
    enum Repeat
    {
        ALL, ONE
    }
}
