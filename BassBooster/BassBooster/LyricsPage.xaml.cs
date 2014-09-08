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
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

//TODO: save/load , usunac z poczatku i konca 2 linijki tekstu, zapisywanie stanu

namespace BassBooster
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LyricsPage : Page
    {
        
        public LyricsPage()
        {
            this.InitializeComponent();            
            BrowserWV.NavigationCompleted += Browser_NavigationCompleted;
            BrowserWV.LoadCompleted += Browser_LoadCompleted;
            
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
                }
                else{
                    LyricsTextBox.Text = await BrowserWV.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('song-text')[0].innerText;" });
                }
            }
            catch (Exception e1)
            {
                LyricsTextBox.Text = "Lyrics weren't found. Check artist or title spelling";
            }
              
            
            BrowserWV.Visibility = Visibility.Collapsed;
        }

        private void LyricsDlButton_Click(object sender, RoutedEventArgs e)
        {
            
            string artist = ArtistBox.Text;
            string title = TitleBox.Text;
            StringParser(ref title);
            StringParser(ref artist);
            BrowserWV.Navigate(new Uri("http://tekstowo.pl/piosenka," + artist + "," + title + ".html"));
        }

        private void StringParser(ref string text){
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
