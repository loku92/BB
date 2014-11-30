using BassBooster.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238



namespace BassBooster
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LyricsPage : Page
    {
        private string CurrentTitle;
        
        public LyricsPage()
        {
            this.InitializeComponent();            
            BrowserWV.NavigationCompleted += Browser_NavigationCompleted;
            BrowserWV.LoadCompleted += Browser_LoadCompleted;
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SuspensionManager.SessionState.ContainsKey("Lyrics"))
            {
                LyricsTextBox.Text = Convert.ToString(SuspensionManager.SessionState["Lyrics"]);
                ArtistBox.Text = Convert.ToString(SuspensionManager.SessionState["Artist"]);
                TitleBox.Text = Convert.ToString(SuspensionManager.SessionState["Title"]);
                base.OnNavigatedTo(e);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SuspensionManager.SessionState["Artist"] = ArtistBox.Text;
            SuspensionManager.SessionState["Title"] = TitleBox.Text;
            SuspensionManager.SessionState["Lyrics"] = LyricsTextBox.Text;
            base.OnNavigatedFrom(e);
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            BrowserWV.Visibility = Visibility.Collapsed;
        }

        private async void Browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            try {                 
                string title = BrowserWV.DocumentTitle;
                //if title wasn't found it looks like "404 - Nie ma takiego pliku! - www.tekstowo.pl   " 
                if (title.Contains("404 - Nie ma takiego pliku!")){
                    LyricsTextBox.Text = "Lyrics weren't found. Check artist or title spelling";
                    SaveLyricsButton.Visibility = Visibility.Collapsed;
                }
                else{
                    string lyrics = await BrowserWV.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('song-text')[0].innerText;" });
                    lyrics = lyrics.Replace("Tekst piosenki:", " ");
                    lyrics = lyrics.Replace("Poznaj historię zmian tego tekstu", " ");
                    LyricsTextBox.Text = lyrics;
                    SuspensionManager.SessionState["Lyrics"] = lyrics;
                    SaveLyricsButton.Visibility = Visibility.Visible;
                }
            }
            catch (Exception e)
            {
                LyricsTextBox.Text = "Lyrics weren't found. Check artist or title spelling";
            }
              
            
            BrowserWV.Visibility = Visibility.Collapsed;
        }

        private void LyricsDlButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTextBox.Text = "Searching...";
            string artist = ArtistBox.Text;
            string title = TitleBox.Text;
            SuspensionManager.SessionState["Artist"] = artist;
            SuspensionManager.SessionState["Title"] = title;
            StringParser(ref title);
            StringParser(ref artist);
            BrowserWV.Navigate(new Uri("http://tekstowo.pl/piosenka," + artist + "," + title + ".html"));
            CurrentTitle = artist + "_" + title;
        }
        

        private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = Windows.Storage.KnownFolders.MusicLibrary;
            StorageFile sampleFile = await folder.CreateFileAsync(CurrentTitle + ".bbf", CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, LyricsTextBox.Text);
            SaveConfTextBlock.Visibility = Visibility.Visible;
        }

        private async void LoadLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker fop = new Windows.Storage.Pickers.FileOpenPicker();
            fop.FileTypeFilter.Add(".bbf");
            fop.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            StorageFile file = await fop.PickSingleFileAsync();
            if (file != null)
            {
                LyricsTextBox.Text = await Windows.Storage.FileIO.ReadTextAsync(file);
                SuspensionManager.SessionState["Lyrics"] = LyricsTextBox.Text;
            }
        }

        private void StringParser(ref string text)
        {
            text = text.Trim();
            text = text.ToLower();
            text = text.Replace(" ", "_");
            text = text.Replace(".", "_");
            text = text.Replace(",", "_");
            text = text.Replace("'", "_");
            text = text.Replace("(", "_");
            text = text.Replace(")", "_");
            text = text.Replace("[", "_");
            text = text.Replace("]", "_");
            text = text.Replace("{", "_");
            text = text.Replace("}", "_");
            text = text.Replace("$", "_");
            text = text.Replace("#", "_");
            text = text.Replace("/", "_");
            text = text.Replace("\\", "_");
            text = text.Replace("@", "_");
            text = text.Replace("#", "_");
            text = text.Replace("&", "_");
            text = text.Replace("*", "_");
            text = text.Replace("+", "_");
            text = text.Replace("!", "_");
            text = text.Replace("?", "_");
            text = text.Replace("~", "_");
            text = text.Replace("-", "_");
            text = text.Replace("ł", "l");
            text = text.Replace("ą", "a");
            text = text.Replace("ę", "e");
            text = text.Replace("ó", "o");
            text = text.Replace("ś", "s");
            text = text.Replace("ć", "c");
            text = text.Replace("ń", "n");
        }           
    }
}
