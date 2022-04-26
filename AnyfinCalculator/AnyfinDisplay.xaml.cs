using AnyfinCalculator.Annotations;
using AnyfinCalculator.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace AnyfinCalculator
{
    /// <summary>
    ///     Interaction logic for AnyfinDisplay.xaml
    /// </summary>
    public partial class AnyfinDisplay : INotifyPropertyChanged
	{
        private readonly Settings Settings;
		
		public AnyfinDisplay(Settings settings)
		{
			InitializeComponent();
			
			Settings = settings;
		}

		private void SetLeft() => Canvas.SetLeft(this, Core.OverlayWindow.Width * Settings.Left / 100);
		private void SetTop() => Canvas.SetTop(this, Core.OverlayWindow.Height * Settings.Top / 100);
		private void SetScale() => RenderTransform = new ScaleTransform(Settings.Scale / 100, Settings.Scale / 100);
		private void SetOpacity() => Opacity = Settings.Opacity / 100;
		public void RefreshDisplay()
        {
			SetTop();
			SetLeft();
			SetScale();
			SetOpacity();
        }

		public string DamageText
		{
			get { return _damageText; }
			set
			{
				if (value == _damageText) return;
				_damageText = value;
				OnPropertyChanged();
			}
		}
		private string _damageText;

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}