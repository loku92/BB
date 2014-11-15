using BassBooster.Common;
using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BassBooster
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OneDriveSyncPage : Page
    {      
        public OneDriveSyncPage()
        {
            this.InitializeComponent();
            bb.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LiveLoginButton_Click(null, null);
        }

        private async void LiveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (OneDriveManager._client == null)
            {
                bool connected = false;
                try
                {
                    var authClient = new LiveAuthClient();
                    LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" });

                    if (result.Status == LiveConnectSessionStatus.Connected)
                    {
                        connected = true;
                        OneDriveManager._client = new LiveConnectClient(result.Session);
                        var meResult = await OneDriveManager._client.GetAsync("me");
                        dynamic meData = meResult.Result;
                    }
                }
                catch (LiveAuthException ex)
                {
                    LiveLoginButton.Content = "Retry";
                }
                catch (LiveConnectException ex)
                {
                    LiveLoginButton.Content = "Retry";
                }                
            }
            LiveLoginButton.Content = "Log out";
            bb.Visibility = Visibility.Visible;
        }
        

        private async void bb_Click(object sender, RoutedEventArgs e)
        {
            OneDriveManager.CreateDirectoryAsync();
        }

    }
}
