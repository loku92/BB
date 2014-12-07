using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BassBooster.Common
{
    /// <summary>
    /// Class for managing Tile notifications
    /// </summary>
    public class TileManager
    {
        public static bool isEnabled = true;


        /// <summary>
        /// Method for updating tile with given data for given duration
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <param name="sec">Duration of the song in milisec</param>
        public static void UpdateTile(string title, int sec)
        {
            if ( TileManager.isEnabled )
            {
                TileManager.UpdateWideTile(title, sec);
                TileManager.UpdateSquareTile(title, sec);
            }
        }

        /// <summary>
        /// 310x150 tile update
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <param name="sec">Duration of the song in milisec</param>
        public static void UpdateWideTile(string title, int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = title;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        /// <summary>
        /// 150x150 Tile update
        /// </summary>
        /// <param name="title">Title of the song</param>
        /// <param name="sec">Duration of the song in milisec</param>

        public static void UpdateSquareTile(string title, int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = title;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static void UpdateSmallSquareTile(string title, int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = title;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static void UpdateBigSquareTile(string title, int sec)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310Text04);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = title;
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(sec);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        /// <summary>
        /// Method to clear tile (sometimes may not work)
        /// </summary>
        public static void ClearTile()
        {
            
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            
        }
    
    }
}
