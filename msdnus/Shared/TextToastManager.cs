using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace msdnus.Shared
{
    public sealed class TextToastManager
    {
        public static TextToastManager Instance { get; } = new TextToastManager();
        private TextToastManager() { }

        public void ShowToast(string line1, string line2 = null)
        {
            try
            {
                var template = $@"
<toast>
<visual>
<binding template=""ToastGeneric"">
  <text>{System.Security.SecurityElement.Escape(line1)}</text>
  {(string.IsNullOrEmpty(line2) ? "" : $"<text>{System.Security.SecurityElement.Escape(line2)}</text>")}
</binding>
</visual>
</toast>
";
                var xml = new XmlDocument();
                xml.LoadXml(template);
                var toast = new ToastNotification(xml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch
            {
                // Fallback if toast fails (Windows App SDK may need additional setup)
                System.Diagnostics.Debug.WriteLine($"Toast: {line1} {line2}");
            }
        }
    }
}
