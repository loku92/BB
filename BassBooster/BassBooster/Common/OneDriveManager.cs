﻿using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace BassBooster.Common
{
    /// <summary>
    /// Class to Manage connection with WindowsLive and synchronizing files with OneDrive
    /// </summary>
    public static class OneDriveManager
    {
        public static LiveConnectClient _client = null;
        public static string _folderId;
        public static System.Threading.CancellationTokenSource CancelToken;
        public static string FOLDER_NAME = "BassBooster";


        /// <summary>
        /// Login into Windows Live 
        /// </summary>
        /// <returns></returns>
        public async static Task SignInOneDriveAsync()
        {
            if (OneDriveManager._client == null)
            {                
                    var authClient = new LiveAuthClient();
                    LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.signin", "wl.skydrive", "wl.skydrive_update" });

                    if (result.Status == LiveConnectSessionStatus.Connected)
                    {
                        OneDriveManager._client = new LiveConnectClient(result.Session);
                    }
            }
        }


        /// <summary>
        /// Checks if directory exists in OneDrive and sets id.
        /// </summary>
        /// <returns>folder id</returns>
        public async static Task<string> GetFolderIdAsync()
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
                result = await CreateDirectoryAsync();
            }
            OneDriveManager._folderId = folderId;
            return folderId;
        }

        /// <summary>
        /// Creates directory in OneDrive
        /// </summary>
        /// <returns>new folder id</returns>

        public async static Task<string> CreateDirectoryAsync()
        {
            var folderData = new Dictionary<string, object>();
            folderData.Add("name", OneDriveManager.FOLDER_NAME);
            LiveOperationResult operationResult = await OneDriveManager._client.PostAsync("me/skydrive", folderData);
            dynamic result = operationResult.Result;
            return result.id;
        }


        /// <summary>
        /// Downloads all files from BBLyrics folder in OneDrive cloud to Music folder
        /// </summary>
        /// <returns></returns>
        public static async Task DownloadFilesAsync(Progress<LiveOperationProgress> progress)
        {
            await CheckIfExists();
            StorageFolder folder = Windows.Storage.KnownFolders.MusicLibrary;
            StorageFile newFile;
            List<string> fileNames = new List<string>();
            string query = _folderId + "/files";
            LiveOperationResult operationResult = await _client.GetAsync(query);
            OneDriveManager.CancelToken = new System.Threading.CancellationTokenSource();
            dynamic result = operationResult.Result;
            foreach (dynamic file in result.data)
            {
                fileNames.Add(file.name); //lame hack not to get runtimebinder exception caused by no getawaiter in object
                newFile = await folder.CreateFileAsync(fileNames[fileNames.Count - 1], CreationCollisionOption.ReplaceExisting);
                await _client.BackgroundDownloadAsync(file.id + "/Content", newFile, OneDriveManager.CancelToken.Token,progress);
            }
            OneDriveManager.CancelToken = null;
        }


        /// <summary>
        /// Uploads selected files to skydrive
        /// </summary>
        /// <returns></returns>
        public static async Task UploadFilesAsync(Progress<LiveOperationProgress> progress)
        {
            await CheckIfExists();
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".bbf");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".aac");
            IReadOnlyList<Windows.Storage.StorageFile> files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                OneDriveManager.CancelToken = new System.Threading.CancellationTokenSource();
                foreach (var file in files)
                {
                    await _client.BackgroundUploadAsync(OneDriveManager._folderId,
                        file.Name, file, Microsoft.Live.OverwriteOption.Overwrite, OneDriveManager.CancelToken.Token, progress);
                }
                OneDriveManager.CancelToken = null;
            }
        }

        /// <summary>
        /// Deletes all data and folder from OneDrive cloud
        /// </summary>
        /// <param name="folderId">id of BBLyrics folder</param>
        /// <returns></returns>
        public static async Task DeleteFolderAsync(string folderId)
        {
            if (_client != null)
            {
                LiveOperationResult operationResult = await _client.DeleteAsync(folderId);
                OneDriveManager._folderId = null;
            }

        }

        /// <summary>
        /// Checks if BBLyrics file exists, if not it creats it
        /// </summary>
        /// <returns></returns>
        public static async Task CheckIfExists()
        {
            if (OneDriveManager._folderId == null)
            {
                await GetFolderIdAsync();
            }
        }

    }
}
