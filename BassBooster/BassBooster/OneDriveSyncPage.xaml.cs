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
    /// Page to manage OneDrive synchronizing.
    /// </summary>
    public sealed partial class OneDriveSyncPage : Page
    {      
        public OneDriveSyncPage()
        {
            this.InitializeComponent();  
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SuspensionManager.SessionState.ContainsKey("Status"))
            {
                StatusBlock.Text = Convert.ToString(SuspensionManager.SessionState["Status"]);
            }
            
            if (OneDriveManager._client == null)
            {
                DownloadButton.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                CancelUpload.Visibility = Visibility.Collapsed;
                StatusBlock.Text = DateTime.Now.ToString("HH:mm") + " Logging into OneDrive.\n"; 
                LiveLoginButton_Click(null, null);
            }
            else
            {
                DownloadButton.Visibility = Visibility.Visible;
                UploadButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                CancelUpload.Visibility = Visibility.Collapsed;
                LiveLoginButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SuspensionManager.SessionState["Status"] = StatusBlock.Text;
        }

        /// <summary>
        /// Live login button action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private async void LiveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LiveLoginButton.Visibility = Visibility.Collapsed;
            try
            {
                await OneDriveManager.SignInOneDriveAsync();
                await OneDriveManager.GetFolderIdAsync();
                DownloadButton.Visibility = Visibility.Visible;
                UploadButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
                StatusBlock.Text = DateTime.Now.ToString("HH:mm") + " Successfully logged in.\n"; 
            }
            catch (LiveAuthException ex)
            {
                LiveLoginButton.Content = "Retry";
                LiveLoginButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                CancelUpload.Visibility = Visibility.Collapsed;
                StatusBlock.Text = DateTime.Now.ToString("HH:mm") + " Failed to log in. Try again. \n";
            }
            catch (LiveConnectException ex)
            {
                LiveLoginButton.Content = "Retry";
                LiveLoginButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                CancelUpload.Visibility = Visibility.Collapsed;
                StatusBlock.Text = DateTime.Now.ToString("HH:mm") + " Failed to log in. Try again. \n";
            }
            catch (NullReferenceException)
            {
                LiveLoginButton.Content = "Retry";
                LiveLoginButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                CancelUpload.Visibility = Visibility.Collapsed;
                StatusBlock.Text = DateTime.Now.ToString("HH:mm") + " Failed to log in. Try again. Did You accept the OneDrive license? \n";
            }
            CancelUpload.Visibility = Visibility.Collapsed;
        }
        
        /// <summary>
        /// Delete button action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await OneDriveManager.GetFolderIdAsync();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Deleting BBLyrics folder please wait...\n";
                await OneDriveManager.DeleteFolderAsync(OneDriveManager._folderId);
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Folder deleted.\n";
            }
            catch (LiveConnectException exception)
            {
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Failed to delete folder, check your Internet connection.\n";    
            }
        }

        /// <summary>
        /// Upload click button action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            UploadButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            DownloadButton.Visibility = Visibility.Collapsed;
            try
            {
                await OneDriveManager.GetFolderIdAsync();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Uploading please wait...\n";
                CancelUpload.Visibility = Visibility.Visible;
                await OneDriveManager.UploadFilesAsync();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Files were uploaded successfully.\n";
            }
            catch (LiveConnectException exception)
            {
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Failed to upload files, check your Internet connection.\n";
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Upload of files was cancelled.\n";                
            }
            finally
            {
                CancelUpload.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

        private void CancelUpload_Click(object sender, RoutedEventArgs e)
        {
            if (OneDriveManager.CancelToken != null)
            {
                OneDriveManager.CancelToken.Cancel();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Action has been cancelled.\n";
            }
        }


        /// <summary>
        /// Button action for downloading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            UploadButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            DownloadButton.Visibility = Visibility.Collapsed;
            try
            {                
                await OneDriveManager.GetFolderIdAsync();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Downloading please wait...\n";
                CancelUpload.Visibility = Visibility.Visible;
                await OneDriveManager.DownloadFilesAsync();
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Files were downloaded successfully to Music Directory\n";
            }
            catch (LiveConnectException exception)
            {
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Failed to download files, check your Internet connection.\n";
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                StatusBlock.Text += DateTime.Now.ToString("HH:mm") + " Download of files was cancelled.\n";
            }
            finally
            {
                CancelUpload.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }

    }
}
