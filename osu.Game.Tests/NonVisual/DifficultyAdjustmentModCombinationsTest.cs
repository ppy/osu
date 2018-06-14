// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class DifficultyAdjustmentModCombinationsTest
    {
        [Test]
        public void TestNoMods()
        {
            var combinations = new TestDifficultyAdjustmentMods().CreateCombinations();

            Assert.AreEqual(1, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
        }

        [Test]
        public void TestSingleMod()
        {
            var combinations = new TestDifficultyAdjustmentMods(new ModA()).CreateCombinations();

            Assert.AreEqual(2, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
            Assert.IsTrue(combinations[1] is ModA);
        }

        [Test]
        public void TestDoubleMod()
        {
            var combinations = new TestDifficultyAdjustmentMods(new ModA(), new ModB()).CreateCombinations();

            Assert.AreEqual(4, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
            Assert.IsTrue(combinations[1] is ModA);
            Assert.IsTrue(combinations[2] is MultiMod);
            Assert.IsTrue(combinations[3] is ModB);

            Assert.IsTrue(((MultiMod)combinations[2]).Mods[0] is ModA);
            Assert.IsTrue(((MultiMod)combinations[2]).Mods[1] is ModB);
        }

        [Test]
        public void TestIncompatibleMods()
        {
            var combinations = new TestDifficultyAdjustmentMods(new ModA(), new ModIncompatibleWithA()).CreateCombinations();

            Assert.AreEqual(3, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
            Assert.IsTrue(combinations[1] is ModA);
            Assert.IsTrue(combinations[2] is ModIncompatibleWithA);
        }

        [Test]
        public void TestDoubleIncompatibleMods()
        {
            var combinations = new TestDifficultyAdjustmentMods(new ModA(), new ModB(), new ModIncompatibleWithA(), new ModIncompatibleWithAAndB()).CreateCombinations();

            Assert.AreEqual(8, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
            Assert.IsTrue(combinations[1] is ModA);
            Assert.IsTrue(combinations[2] is MultiMod);
            Assert.IsTrue(combinations[3] is ModB);
            Assert.IsTrue(combinations[4] is MultiMod);
            Assert.IsTrue(combinations[5] is ModIncompatibleWithA);
            Assert.IsTrue(combinations[6] is MultiMod);
            Assert.IsTrue(combinations[7] is ModIncompatibleWithAAndB);

            Assert.IsTrue(((MultiMod)combinations[2]).Mods[0] is ModA);
            Assert.IsTrue(((MultiMod)combinations[2]).Mods[1] is ModB);
            Assert.IsTrue(((MultiMod)combinations[4]).Mods[0] is ModB);
            Assert.IsTrue(((MultiMod)combinations[4]).Mods[1] is ModIncompatibleWithA);
            Assert.IsTrue(((MultiMod)combinations[6]).Mods[0] is ModIncompatibleWithA);
            Assert.IsTrue(((MultiMod)combinations[6]).Mods[1] is ModIncompatibleWithAAndB);
        }

        [Test]
        public void TestIncompatibleThroughBaseType()
        {
            var combinations = new TestDifficultyAdjustmentMods(new ModAofA(), new ModIncompatibleWithAofA()).CreateCombinations();

            Assert.AreEqual(3, combinations.Length);
            Assert.IsTrue(combinations[0] is NoModMod);
            Assert.IsTrue(combinations[1] is ModAofA);
            Assert.IsTrue(combinations[2] is ModIncompatibleWithAofA);
        }

        private class ModA : Mod
        {
            public override string Name => nameof(ModA);
            public override string ShortenedName => nameof(ModA);
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithA), typeof(ModIncompatibleWithAAndB) };
        }

        private class ModB : Mod
        {
            public override string Name => nameof(ModB);
            public override string ShortenedName => nameof(ModB);
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithAAndB) };
        }

        private class ModIncompatibleWithA : Mod
        {
            public override string Name => $"Incompatible With {nameof(ModA)}";
            public override string ShortenedName => $"Incompatible With {nameof(ModA)}";
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModA) };
        }

        private class ModAofA : ModA
        {
        }

        private class ModIncompatibleWithAofA : ModIncompatibleWithA
        {
            // Incompatible through base type
        }

        private class ModIncompatibleWithAAndB : Mod
        {
            public override string Name => $"Incompatible With {nameof(ModA)} and {nameof(ModB)}";
            public override string ShortenedName => $"Incompatible With {nameof(ModA)} and {nameof(ModB)}";
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModA), typeof(ModB) };
        }

        private class TestDifficultyAdjustmentMods : DifficultyAdjustmentMods
        {
            public TestDifficultyAdjustmentMods(params Mod[] mods)
            {
                Mods = mods;
            }

            protected override Mod[] Mods { get; }
        }
    }
}
