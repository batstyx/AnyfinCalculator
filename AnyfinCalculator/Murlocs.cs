using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker.Hearthstone;
using static HearthDb.CardIds;

namespace AnyfinCalculator
{
	//MRGLGLG
	public static class Murlocs
	{
		static Murlocs()
		{
			BluegillWarrior = Database.GetCardFromId(Collectible.Neutral.BluegillWarriorLegacy);
			GrimscaleOracle = Database.GetCardFromId(Collectible.Neutral.GrimscaleOracleLegacy);
			MurlocWarleader = Database.GetCardFromId(Collectible.Neutral.MurlocWarleaderExpert1);
			OldMurkEye = Database.GetCardFromId(Collectible.Neutral.OldMurkEyeLegacy);
			MurlocTidecaller = Database.GetCardFromId(Collectible.Neutral.MurlocTidecaller);
			LushwaterScout = Database.GetCardFromId(Collectible.Neutral.LushwaterScout);
			AnyfinCanHappen = Database.GetCardFromId(Collectible.Paladin.AnyfinCanHappen);			
		}

		public static Card BluegillWarrior { get; }
		public static Card GrimscaleOracle { get; }
		public static Card MurlocWarleader { get; }
		public static Card OldMurkEye { get; }
		public static Card MurlocTidecaller { get; }
		public static Card LushwaterScout { get; }
		public static Card AnyfinCanHappen { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsMurloc(this Card card) => card.Race == "Murloc" || card.Race == "All";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsChargeMurloc(this Card card) => card.Id == OldMurkEye.Id || card.Id == BluegillWarrior.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBluegill(this Card card) => card.Id == BluegillWarrior.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGrimscale(this Card card) => card.Id == GrimscaleOracle.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWarleader(this Card card) => card.Id == MurlocWarleader.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsMurkEye(this Card card) => card.Id == OldMurkEye.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsTidecaller(this Card card) => card.Id == MurlocTidecaller.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsScout(this Card card) => card.Id == LushwaterScout.Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAnyfin(this Card card) => card.Id == AnyfinCanHappen.Id;
	}
}