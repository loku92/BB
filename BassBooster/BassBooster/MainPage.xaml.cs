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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BassBooster
{
    /// <summary>
    /// MediaElement doesn't support FLAC
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Vars
        public static string CurrentlyPlaying;
        private bool empty; // true if there aren't any tracks loaded
        private int? CurrentList;
        private int? CurrentTrack;
        private List<IReadOnlyList<Windows.Storage.StorageFile>> Playlist = null;
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
            MP3Player.Volume = 1.0;
            TrackListBox.ItemsSource = null;
            empty = true;
            
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
                SuspensionManager.SessionState["CurrentTab"] = tabList.SelectedIndex;
                TabFrame.Navigate(t.ClassType);
            }
        }

        #region PlayerManagement

        private void MediaControl_ntp(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private void MediaControl_sp(object sender, object e)
        {
            throw new NotImplementedException();
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

        private void MediaControl_plp(object sender, object e)
        {
            //
            throw new NotImplementedException();
        }


        private void MediaControl_pp(object sender, object e)
        {
            throw new NotImplementedException();
        }

        private void MP3Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MP3Player != null)
                MP3Player.Volume = ((double)VolumeSlider.Value) / 100.0;
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
        
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MP3Player.CurrentState == MediaElementState.Playing){
                MP3Player.Pause();
                PlayButton.Icon = new SymbolIcon(Symbol.Play);
                
            }
            else if(!empty && (MP3Player.CurrentState == MediaElementState.Paused || MP3Player.CurrentState == MediaElementState.Stopped ))
            {
                MP3Player.Play();
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            }
            //TabList.Add(new Tab { Name = "aaa", ClassType = typeof(ListPage) });
            //TrackListBox.ItemsSource = null;
            //TrackListBox.ItemsSource = TabList;
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
                int currentPlIndex = Playlist.Count - 1;
                //add 'miniplaylist' ranges
                Tracklist.RangeAdd(currentPlIndex, files.Count);
                int i = 0;
                foreach (var f in files)
                {
                    musicProperties = await f.Properties.GetMusicPropertiesAsync();
                    Tracklist.Add(new Track (i,currentPlIndex,musicProperties.Artist,musicProperties.Title,f.Name,musicProperties.Duration));
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
                    TitleBox.Text = Tracklist.TrackToString(0);
                }
                //refresh track listbox
                TrackListBox.ItemsSource = null;
                TrackListBox.ItemsSource = Tracklist.Music;
                empty = false;
                MP3Player.Play();
                //set button icon to pause
                PlayButton.Icon = new SymbolIcon(Symbol.Pause);                
               
            }  
        } 
    }

    private enum REPEAT
    {
        NONE,ALL,ONE
    }
}
