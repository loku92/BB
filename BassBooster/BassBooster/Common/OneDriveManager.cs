using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BassBooster.Common
{
    public class OneDriveManager
    {
        public static LiveConnectClient _client = null;
        public static string _folderId;
        public static System.Threading.CancellationTokenSource _cancelUpload;
        private const string FOLDER_NAME = "BBLyrics";
        public const string EXTENSION = ".bbf";

        public async static Task<int> SignInOneDrive(){
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
                    return -1;
                }
                catch (LiveConnectException ex)
                {
                    return -1;
                }                
            }
            return 0;
        }



        public async static Task<string> CreateDirectoryAsync()
        {
            string folderId = null;
            var query = "me/skydrive/files?filter=folders,albums";
            var opResult = await OneDriveManager._client.GetAsync(query);
            dynamic result = opResult.Result;

            foreach (dynamic folder in result.data)
            {
                if (folder.name.ToLowerInvariant() == OneDriveManager.FOLDER_NAME.ToLowerInvariant())
                {
                    folderId = folder.id;
                    break;
                }
            }

            if (folderId == null)
            {
                try
                {
                    var folderData = new Dictionary<string, object>();
                    folderData.Add("name", OneDriveManager.FOLDER_NAME);
                    LiveOperationResult operationResult = await OneDriveManager._client.PostAsync("me/skydrive", folderData);
                    result = operationResult.Result;
                    return result.id;
                }
                catch (LiveConnectException exc)
                {
                    return null;
                }
            }
            OneDriveManager._folderId = folderId;
            return folderId;
        }

        public static async Task DownloadFiles()
        {
            CheckIfExists();
            StorageFolder folder = Windows.Storage.KnownFolders.MusicLibrary;
            StorageFile newFile;
            List<string> fileNames= new List<string>();
            try
            {
                CheckIfExists();
                string query = _folderId + "/files";
                LiveOperationResult operationResult =  await _client.GetAsync(query);
                dynamic result = operationResult.Result;
                foreach (dynamic file in result.data)
                {             
                    fileNames.Add( file.name ); //lame hack no to get runtimebinder exception caused by no getawaiter in object
                    newFile = await folder.CreateFileAsync(fileNames[fileNames.Count-1], CreationCollisionOption.ReplaceExisting);
                    await _client.BackgroundDownloadAsync(file.id+"/Content",newFile);                    
                }

            }
            catch (LiveConnectException exception)
            {
            }        
        }

        public static async Task UploadFiles()
        {
            try
            {
                CheckIfExists();
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                picker.FileTypeFilter.Add(".bbf");
                IReadOnlyList<Windows.Storage.StorageFile> files = await picker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    var progressHandler = new Progress<LiveOperationProgress>(
                        (progress) => {  });
                    OneDriveManager._cancelUpload = new System.Threading.CancellationTokenSource();
                    foreach (var file in files)
                    {
                        await _client.BackgroundUploadAsync(OneDriveManager._folderId,
                            file.Name, file, Microsoft.Live.OverwriteOption.Overwrite, OneDriveManager._cancelUpload.Token, progressHandler);
                    }
                        
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
            }
            catch (LiveConnectException exception)
            {
            }
        }

        public static async Task DeleteFolder(string folderId)
        {
            if (_client != null)
            {
                LiveOperationResult operationResult = await _client.DeleteAsync(folderId);
            }

        }
                
        public static async void CheckIfExists()
        {
            if (OneDriveManager._folderId == null)
            {
                await CreateDirectoryAsync();
            }
        }

    }
}
