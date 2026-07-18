using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace msdnus.Shared
{
    public sealed class TextToastManager
    {
        public static TextToastManager Instance { get; } = new TextToastManager();
        private TextToastManager() { }

        public void ShowToast(string line1, string? line2 = null)
        {
            try
            {
                var builder = new AppNotificationBuilder().AddText(line1);
                if (!string.IsNullOrEmpty(line2))
                {
                    builder.AddText(line2);
                }

                AppNotificationManager.Default.Show(builder.BuildNotification());
            }
            catch
            {
                // Notifications require package identity or explicit registration;
                // never let a toast failure break the calling feature.
                System.Diagnostics.Debug.WriteLine($"Toast: {line1} {line2}");
            }
        }
    }
}
