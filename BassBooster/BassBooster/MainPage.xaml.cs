using BassBooster.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            
        }        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavTab.ItemsSource = TabList;
        }

        #region PlayerManagement
        #endregion

        #region Tabs


        public List<Tab> TabList = new List<Tab>
        {
            new Tab() { Name = "Playlist", ClassType = typeof(ListPage) },
            new Tab() { Name = "Lyrics", ClassType = typeof(LyricsPage) },
            new Tab() { Name = "OneDrive - synchronizing lyrics.", ClassType = typeof(OneDriveSyncPage) }
        };

        public List<Tab> Tabs
        {
            get { return this.TabList; }
        }



        #endregion
    }
}
