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
        public LiveConnectClient client ;
        public LiveAuthClient authClient;
        public string FolderName
        {
            get { return Package.Current.Id.Name; }
        }

        ApplicationDataContainer settings = ApplicationData.Current.RoamingSettings;
        public string FolderId
        {
            get { return this.settings.Values["FolderId"].ToString(); }
            private set { this.settings.Values["FolderId"] = value; }
        }

        public OneDriveSyncPage()
        {
            this.InitializeComponent();
            authClient = new LiveAuthClient();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LiveLoginButton_Click(null, null);
        }

        private async void LiveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!MainPage.LoggedIn) { 
            bool connected = false;
            try
            {
                
                LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.signin", "wl.skydrive" });

                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    connected = true;
                    client = new LiveConnectClient(result.Session);
                    var meResult = await client.GetAsync("me");
                    dynamic meData = meResult.Result;
                }
            }
            catch (LiveAuthException ex)
            {
                StatusBlock.Text = "LiveAuthException";
                Debug.WriteLine("Exception during sign-in: {0}", ex.Message);
            }
            catch (LiveConnectException ex)
            {
                StatusBlock.Text = "LiveConnectException";
            }
           // MainPage.LoggedIn = true;
            LiveLoginButton.Content = "Log out";
            //}
            
        }
        private async void CreateFolder_Executed()
        {
            try
            {
                // The overload with a String expects JSON, so this does not work:
                // LiveOperationResult lor = await client.PostAsync("me/skydrive", Package.Current.Id.Name);

                // The overload with a Dictionary accepts initializers:
                LiveOperationResult lor = await client.PostAsync("me/skydrive", new Dictionary<string, object>() { { "name", this.FolderName } });
                dynamic result = lor.Result;
                string name = result.name;
                string id = result.id;
                this.FolderId = id;
                Debug.WriteLine("Created '{0}' with id '{1}'", name, id);
            }
            catch (LiveConnectException ex)
            {
                if (ex.HResult == -2146233088)
                {
                    Debug.WriteLine("The folder already existed.");
                }
                else
                {
                    Debug.WriteLine("Exception during folder creation: {0}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                // Get the code monkey's attention.
                Debugger.Break();
            }
        }

        private void bb_Click(object sender, RoutedEventArgs e)
        {
            CreateFolder_Executed();
        }

    }
}
