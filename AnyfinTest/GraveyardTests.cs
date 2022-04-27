using AnyfinCalculator;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AnyfinTest
{
    [TestClass]
    public class GraveyardTests
    {
        private List<Card> Graveyard;
        private List<Entity> PlayerBoard;
        private List<Entity> OpponentBoard;

        [TestInitialize]
        public void InitializeTestGraveyard()
        {
            Graveyard = new List<Card>();
            PlayerBoard = new List<Entity>();
            OpponentBoard = new List<Entity>();
        }

        private void AddCard(Card card) => Graveyard.Add(card.Clone() as Card);
        private void AddBluegill() => AddCard(Murlocs.BluegillWarrior);
        private void AddMurkEye() => AddCard(Murlocs.OldMurkEye);
        private void AddGrimscale() => AddCard(Murlocs.GrimscaleOracle);
        private void AddWarleader() => AddCard(Murlocs.MurlocWarleader);
        private void AddScout() => AddCard(Murlocs.LushwaterScout);

        private int GetDamage()
        {
            return DamageCalculator.CalculateDamageInternal(
                Graveyard,
                PlayerBoard,
                OpponentBoard);
        }

        [TestMethod]
        public void Test_Bluegill()
        {
            AddBluegill();
            var damage = GetDamage();

            Assert.AreEqual(2, damage);
        }

        [TestMethod]
        public void Test_Bluegill_Grimscale()
        {
            AddBluegill();
            AddGrimscale();
            var damage = GetDamage();

            Assert.AreEqual(3, damage);
        }

        [TestMethod]
        public void Test_Bluegill_Warleader_Grimscale()
        {
            AddBluegill();
            AddWarleader();
            AddGrimscale();
            var damage = GetDamage();

            Assert.AreEqual(5, damage);
        }

        [TestMethod]
        public void Test_Bluegill_Warleader()
        {
            AddBluegill();
            AddWarleader();
            var damage = GetDamage();

            Assert.AreEqual(4, damage);
        }

        [Ignore, TestMethod]
        public void Test_Bluegill_Scout()
        {
            AddBluegill();
            AddScout();
            var damage = GetDamage();

            Assert.AreEqual(2, damage);
        }

        [Ignore, TestMethod]
        public void Test_Scout_Bluegill()
        {
            AddScout();
            AddBluegill();
            var damage = GetDamage();

            Assert.AreEqual(3, damage);
        }

        [TestMethod]
        public void Test_MurkEye()
        {
            AddMurkEye();
            var damage = GetDamage();

            Assert.AreEqual(2, damage);
        }

        [TestMethod]
        public void Test_MurkEye_Bluegill()
        {
            AddMurkEye();
            AddBluegill();
            var damage = GetDamage();

            Assert.AreEqual(5, damage);
        }

        [TestMethod]
        public void Test_MurkEye_Grimscale()
        {
            AddMurkEye();
            AddGrimscale();
            var damage = GetDamage();

            Assert.AreEqual(4, damage);
        }

        [TestMethod]
        public void Test_MurkEye_Warleader()
        {
            AddMurkEye();
            AddWarleader();
            var damage = GetDamage();

            Assert.AreEqual(5, damage);
        }

        [TestMethod]
        public void Test_MurkEye_Warleader_Grimscale()
        {
            AddMurkEye();
            AddWarleader();
            AddGrimscale();
            var damage = GetDamage();

            Assert.AreEqual(7, damage);
        }

        [Ignore,TestMethod]
        public void Test_MurkEye_Scout()
        {
            AddMurkEye();
            AddScout();
            var damage = GetDamage();

            Assert.AreEqual(3, damage);
        }

        [Ignore, TestMethod]
        public void Test_Scout_MurkEye()
        {
            AddScout();
            AddMurkEye();
            var damage = GetDamage();

            Assert.AreEqual(4, damage);
        }

        [TestMethod]
        public void Test_Murkeye_2xBlueGill_2xWarleader_2xGrimscale()
        {
            AddMurkEye();
            AddBluegill();
            AddBluegill();
            AddWarleader();
            AddWarleader();
            AddGrimscale();
            AddGrimscale();
            var damage = GetDamage();

            Assert.AreEqual(30, damage);
        }

        [Ignore,TestMethod]
        public void Test_Scout_Murkeye_BlueGill()
        {
            AddScout();
            AddMurkEye();
            AddBluegill();
            var damage = GetDamage();

            Assert.AreEqual(8, damage);
        }

        [Ignore, TestMethod]
        public void Test_Murkeye_Scout_BlueGill()
        {            
            AddMurkEye();
            AddScout();
            AddBluegill();
            var damage = GetDamage();

            Assert.AreEqual(7, damage);
        }
    }
}
