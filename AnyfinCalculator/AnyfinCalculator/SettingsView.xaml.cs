using AnyfinCalculator.Properties;
using Hearthstone_Deck_Tracker;
using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace AnyfinCalculator
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Grid
    {        
        public static Flyout Flyout => _Flyout ?? (_Flyout = CreateSettingsFlyout());
        private static Flyout _Flyout;

        private static Flyout CreateSettingsFlyout()
        {
            var settings = new Flyout
            {
                Position = Position.Left,
                Header = Strings.Get("SettingsTitle"),
                Content = new SettingsView()
            };
            Panel.SetZIndex(settings, 100);
            Core.MainWindow.Flyouts.Items.Add(settings);
            return settings;
        }

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
