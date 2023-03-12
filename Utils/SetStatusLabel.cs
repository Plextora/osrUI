using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace osrUI.Utils
{
    public class SetStatusLabel
    {
        public static void Default(Label? statusLabel)
        {
            if (statusLabel == null) return;
            statusLabel.Foreground = Brushes.White;
            statusLabel.Content = "Status: Idle";
        }

        public static void Pending(Label? statusLabel, string statusText)
        {
            if (statusLabel == null) return;
            statusLabel.Foreground = Brushes.Khaki;
            statusLabel.Content = $"Status: {statusText}";
        }

        public static void Completed(Label? statusLabel, string statusText)
        {
            if (statusLabel == null) return;
            statusLabel.Foreground = Brushes.LightGreen;
            statusLabel.Content = $"Status: {statusText}";
        }

        public static void Error(Label? statusLabel, string statusText)
        {
            if (statusLabel == null) return;
            statusLabel.Foreground = Brushes.Crimson;
            statusLabel.Content = $"Status: {statusText}";
        }
    }
}
