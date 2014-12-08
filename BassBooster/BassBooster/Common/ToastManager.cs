using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BassBooster.Common
{

    /// <summary>
    /// Class for managing toast notifications
    /// </summary>
    public static class ToastManager
    {
        public static bool isEnabled = false;

        /// <summary>
        /// Method to show toast with given string
        /// </summary>
        /// <param name="title">Title of a song</param>
        public static void ShowToast(string title)
        {
            if (isEnabled)
            {
                ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                ((XmlElement)toastNode).SetAttribute("duration", "short");
                ToastNotification toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }
    }
}
