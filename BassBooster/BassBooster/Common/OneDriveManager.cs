using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassBooster.Common
{
    public class OneDriveManager
    {
        public static LiveConnectClient _client = null;

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
            string folderName = "BBLyrics";
            var query = "me/skydrive/files?filter=folders,albums";
            var opResult = await OneDriveManager._client.GetAsync(query);
            dynamic result = opResult.Result;

            foreach (dynamic folder in result.data)
            {
                if (folder.name.ToLowerInvariant() == folderName.ToLowerInvariant())
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
                    folderData.Add("name", "BBLyrics");
                    LiveOperationResult operationResult = await OneDriveManager._client.PostAsync("me/skydrive", folderData);
                    result = operationResult.Result;
                    return result.id;
                }
                catch (LiveConnectException exc)
                {
                    return null;
                }
            }

            return folderId;
        }

        //public async static Task<string> DownloadFileAsync(LiveConnectClient client, string directory, string fileName)
        //{
        //    string skyDriveFolder = await CreateOrGetDirectoryAsync(client, directory, "me/skydrive");
        //    var result = await client.BackgroundDownloadAsync(skyDriveFolder);

        //    var operation = await client.GetAsync(skyDriveFolder + "/files");

        //    var items = operation.Result["data"] as List<object>;
        //    string id = string.Empty;

        //    // Search for the file - add handling here if File Not Found
        //    foreach (object item in items)
        //    {
        //        IDictionary<string, object> file = item as IDictionary<string, object>;
        //        if (file["name"].ToString() == fileName)
        //        {
        //            id = file["id"].ToString();
        //            break;
        //        }
        //    }

        //    var downloadResult = await client.GetAsync(string.Format("{0}/content", id));

        //    //var reader = new StreamReader(downloadResult.Stream);
        //    //string text = await reader.ReadToEndAsync();
        //    return null;
        //}
    }
}
