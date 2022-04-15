using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;
using System.Threading.Tasks;
using AnyfinCalculator.Properties;

namespace AnyfinCalculator
{
	public class AnyfinPlugin : IPlugin
	{
		private Settings Settings;

		private DamageCalculator Calculator;
		private HearthstoneTextBlock _displayBlock;
		private AnyfinDisplay Display;
		//todo: this
		//private CardToolTip _toolTip;
		private StackPanel _toolTipsPanel;
		private bool InAnyfinGame;

		internal bool DeckHasAnyfin => DeckList.Instance?.ActiveDeck.Cards.Contains(Murlocs.AnyfinCanHappen) ?? false;
		internal bool IsInvalidMatch => Core.Game.IsBattlegroundsMatch || Core.Game.IsMercenariesMatch;
		internal bool ShowInMenu => Core.Game.IsInMenu && !Config.Instance.HideInMenu;

		private Visibility Visibility => ShowInMenu || InAnyfinGame
			? Visibility.Visible : Visibility.Collapsed;

		protected MenuItem MainMenuItem { get; set; }

		public void OnLoad()
		{
			Settings = Settings.Default;

			_displayBlock = new HearthstoneTextBlock { FontSize = 36, Visibility = Visibility.Collapsed };
			Calculator = new DamageCalculator();
			Display = new AnyfinDisplay(Settings) { Visibility = Visibility.Collapsed };
			//_toolTip = new CardToolTip();
			_toolTipsPanel = new StackPanel();

			// Create main menu item
			MainMenuItem = new MenuItem {  Header = Strings.Get("MenuTitle") };
			
			var calcMenuItem = new MenuItem { Header = Strings.Get("MenuCalculate") };
			calcMenuItem.Click += new RoutedEventHandler(ForceUpdateClick);
			MainMenuItem.Items.Add(calcMenuItem);

			var settingsMenuItem = new MenuItem { Header = Strings.Get("MenuSettings") };
			settingsMenuItem.Click += (sender, args) => OnButtonPress();
			MainMenuItem.Items.Add(settingsMenuItem);

			GameEvents.OnPlayerHandMouseOver.Add(OnMouseOver);
			GameEvents.OnMouseOverOff.Add(OnMouseOff);
			GameEvents.OnGameStart.Add(OnGameStart);
			GameEvents.OnGameEnd.Add(OnGameEnd);
			GameEvents.OnOpponentPlayToGraveyard.Add(UpdateDisplay);
			GameEvents.OnPlayerPlayToGraveyard.Add(UpdateDisplay);
			GameEvents.OnPlayerPlay.Add(UpdateDisplay);
			GameEvents.OnOpponentPlay.Add(UpdateDisplay);
			DeckManagerEvents.OnDeckSelected.Add(OnGameStart);
			GameEvents.OnTurnStart.Add(OnTurnStart);
			Core.OverlayCanvas.Children.Add(Display);
		}

		#region New Mode

		private void OnGameEnd()
		{
			InAnyfinGame = false;
		}

		private void OnGameStart(Deck obj) => OnGameStart();

		private void OnGameStart()
		{
			if (IsInvalidMatch) return;
			InAnyfinGame = DeckHasAnyfin;
		}

		private void ForceUpdateClick(object o, RoutedEventArgs a)
		{
			UpdateDisplay(null);
		}

		private void OnTurnStart(ActivePlayer player)
		{
			UpdateDisplay(null);
		}

		private bool Updating = false;
		private async void UpdateDisplay(Card c)
		{			
			if (!InAnyfinGame || Updating) return;
			Updating = true;

			// Temporary fix for race condition where the GameTags on the played card haven't been updated yet.
			await Task.Delay(200);

			Range<int> range = Calculator.CalculateDamageDealt();
			Display.DamageText = $"{range}";
			Updating = false;
		}

		#endregion

		public void OnButtonPress() => SettingsView.Flyout.IsOpen = true;

		public void OnUnload()
		{
			MainMenuItem = null;

			Core.OverlayCanvas.Children.Remove(Display);
			Display = null;

			if (Settings?.HasChanges ?? false) Settings.Save();
			Settings = null;
		}

		public void OnUpdate()
		{
            if (Display != null)
            {
				Display.Visibility = Visibility;
			}			
		}

		public string Name => "Anyfin Can Happen Calculator";

		public string Description
			=>
				"Anyfin Can Happen Calculator is a plugin for Hearthstone Deck Tracker which allows you to quickly and easily figure out the damage " +
				"(or damage range) that playing Anyfin Can Happen will have on your current board. \n For any questions or issues look at github.com/ericBG/AnyfinCalculator";

		public MenuItem MenuItem
		{
			get { return MainMenuItem; }
		}

		public string ButtonText => Strings.Get("PluginButton");
		public string Author => "ericBG";
		public Version Version => LibraryInfo.Version;

		#region Classic Mode

		private void OnMouseOff()
		{
			_displayBlock.Visibility = Visibility.Collapsed;
			Core.OverlayCanvas.Children.Remove(_displayBlock);
		}

		private void PlaceTextboxWithText(string text)
		{
			_displayBlock.Text = text;
			_displayBlock.Visibility = Visibility.Visible;
			if (!Core.OverlayCanvas.Children.Contains(_displayBlock))
				Core.OverlayCanvas.Children.Add(_displayBlock);
			Log.Debug($"Textbox has been placed to display:\n '{text}'.");
		}

		private void OnMouseOver(Card card)
		{
			if (!card.IsAnyfin() || !Settings.Default.ClassicMode) return;
			Log.Debug("Anyfin hover detected");
			var damageDealt = Calculator.CalculateDamageDealt();
			var friendlyText = damageDealt.Minimum == damageDealt.Maximum ? "" : "between ";
			PlaceTextboxWithText($"Anyfin can deal {friendlyText}{damageDealt}");
		}

		#endregion
	}
}
