using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AnyfinCalculator
{
	public class DamageCalculator
	{
		private readonly GraveyardHelper _helper = new GraveyardHelper(c => c.IsMurloc());

		public IEnumerable<Card> DeadMurlocs => _helper.TrackedMinions;

		public Range<int> CalculateDamageDealt()
		{
			var deadMurlocs = DeadMurlocs.ToList();
			var board = Core.Game.Player.Board.Where(c => c.Card.IsMurloc()).ToList();
			var opponent = Core.Game.Opponent.Board.Where(c => c.Card.IsMurloc()).ToList();
			if (Core.Game.PlayerMinionCount >= 7) return new Range<int>() {Maximum = 0, Minimum = 0};
			if (deadMurlocs.Count() + Core.Game.PlayerMinionCount <= 7)
			{
				var damage = CalculateDamageInternal(deadMurlocs, board, opponent);
				return new Range<int> {Maximum = damage, Minimum = damage};
			}

			var sw = Stopwatch.StartNew();		
			var whatsTheDamage = new ConcurrentBag<int>();
			var combinations = Combinations(deadMurlocs, 7 - Core.Game.PlayerMinionCount);
			Parallel.ForEach(combinations, combination => whatsTheDamage.Add(CalculateDamageInternal(combination, board, opponent)));				
			sw.Stop();
			var damageRange = new Range<int> { Maximum = whatsTheDamage.Max(), Minimum = whatsTheDamage.Min() };
			Log.Info($"Damage range {damageRange}. Time to calculate {whatsTheDamage.Count} possibilities: {sw.Elapsed:ss\\:fff}");
			return damageRange;
		}

		private static int CalculateDamageInternal(IEnumerable<Card> graveyard, IEnumerable<Entity> friendlyBoard,
			IEnumerable<Entity> opponentBoard)
		{
			var deadMurlocs = graveyard;
			var aliveMurlocs = friendlyBoard;
			var opponentMurlocs = opponentBoard;
			//compiles together into one big freaking list
			var murlocs =
				deadMurlocs.Select(
					c =>
						new MurlocInfo
						{
							AreBuffsApplied = false,
							Attack = c.Attack,
							BoardState = MurlocInfo.State.Dead,
							CanAttack = c.IsChargeMurloc(),
							IsSilenced = false,
							Murloc = c
						})
					.Concat(
						aliveMurlocs.Select(
							ent =>
								new MurlocInfo
								{
									AreBuffsApplied = true,
									Attack = ent.GetTag(GameTag.ATK),
									BoardState = MurlocInfo.State.OnBoard,
									CanAttack = CanAttack(ent),
									IsSilenced = IsSilenced(ent),
									Murloc = ent.Card
								}))
					.Concat(
						opponentMurlocs.Select(
							ent =>
								new MurlocInfo
								{
									AreBuffsApplied = false,
									Attack = ent.Card.Attack,
									BoardState = MurlocInfo.State.OnOpponentsBoard,
									CanAttack = false,
									IsSilenced = IsSilenced(ent),
									Murloc = ent.Card
								})).ToList();

			// Calculate which of the murlocs give buffs (now only your own murlocs)
			var nonSilencedWarleaders =
				murlocs.Count(m => m.BoardState == MurlocInfo.State.OnBoard && m.Murloc.IsWarleader() && !m.IsSilenced);
			var nonSilencedGrimscales =
				murlocs.Count(m => m.BoardState == MurlocInfo.State.OnBoard && m.Murloc.IsGrimscale() && !m.IsSilenced);

			// Get the murlocs that will be summoned
			var murlocsToBeSummoned = murlocs.Count(m => m.BoardState == MurlocInfo.State.Dead);
			// Accumulative Lushwater Scout bonus
			var murlocScoutBonus = 0;

			// Go through each currently buffed murloc and remove the buffs
			foreach (var murloc in murlocs.Where(t => t.AreBuffsApplied))
			{
				murloc.AreBuffsApplied = false;
				murloc.Attack -= nonSilencedGrimscales + (nonSilencedWarleaders*2) + murlocScoutBonus;
				if (murloc.IsSilenced) continue;
				if (murloc.Murloc.IsGrimscale()) murloc.Attack += 1;
				if (murloc.Murloc.IsWarleader()) murloc.Attack += 2;
				if (murloc.Murloc.IsMurkEye()) murloc.Attack -= (murlocs.Count(m => m.BoardState != MurlocInfo.State.Dead) - 1);
				if (murloc.Murloc.IsScout()) murlocScoutBonus += 1;
			}

			// Add the now summoned buffers to the pool
			nonSilencedWarleaders += murlocs.Count(m => m.BoardState == MurlocInfo.State.Dead && m.Murloc.IsWarleader());
			nonSilencedGrimscales += murlocs.Count(m => m.BoardState == MurlocInfo.State.Dead && m.Murloc.IsGrimscale());

			// Reset Lushwater Scout bonus
			murlocScoutBonus = 0;

			// Go through the murlocs on the board and apply all of the final buffs
			foreach (var murloc in murlocs)
			{
				murloc.AreBuffsApplied = true;
				murloc.Attack += nonSilencedGrimscales + (nonSilencedWarleaders*2) + murlocScoutBonus;
				if (murloc.IsSilenced) continue;
				if (murloc.Murloc.IsWarleader()) murloc.Attack -= 2;
				if (murloc.Murloc.IsGrimscale()) murloc.Attack -= 1;
				if (murloc.Murloc.IsMurkEye()) murloc.Attack += (murlocs.Count - 1);
				if (murloc.Murloc.IsTidecaller()) murloc.Attack += murlocsToBeSummoned;
				if (murloc.Murloc.IsScout()) murlocScoutBonus += 1;
			}
			Log.Debug(murlocs.Aggregate("",
				(s, m) =>
					s + $"{m.Murloc.Name}{(m.IsSilenced ? " (Silenced)" : "")}: {m.Attack} {(!m.CanAttack ? "(Can't Attack)" : "")}\n"));
			return murlocs.Sum(m => m.CanAttack ? m.Attack : 0);
		}

		private static bool IsSilenced(Entity entity) => entity.GetTag(GameTag.SILENCED) == 1;

		private static bool CanAttack(Entity entity)
		{
			if (entity.GetTag(GameTag.CANT_ATTACK) == 1 || entity.GetTag(GameTag.FROZEN) == 1)
				return false;
			if (entity.GetTag(GameTag.EXHAUSTED) == 1)
				//from reading the HDT source, it seems like internally Charge minions still have summoning sickness
				return entity.GetTag(GameTag.CHARGE) == 1 &&
				       entity.GetTag(GameTag.NUM_ATTACKS_THIS_TURN) < MaxAttacks(entity);
			return entity.GetTag(GameTag.NUM_ATTACKS_THIS_TURN) < MaxAttacks(entity);
		}

		private static int MaxAttacks(Entity entity)
		{
			// GVG_111t == V-07-TR-0N (MegaWindfury, 4x attack)
			if (entity.CardId == "GVG_111t") return 4;
			// if it has windfury it can attack twice, else it can only attack once
			return entity.GetTag(GameTag.WINDFURY) == 1 ? 2 : 1;
		}

		public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int k)
		{
			return k == 0
				? new[] {new T[0]}
				: elements.SelectMany((e, i) =>
					Combinations(elements.Skip(i + 1), k - 1).Select(c => (new[] {e}).Concat(c)));
		}
	}
}
