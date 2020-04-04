// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class DifficultyAdjustmentModCombinationsTest
    {
        [Test]
        public void TestNoMods()
        {
            var combinations = new TestLegacyDifficultyCalculator().CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(1, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
        }

        [Test]
        public void TestSingleMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA()).CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(2, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
            Assert.IsTrue(combinations[1] is ModA);
        }

        [Test]
        public void TestDoubleMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModB()).CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(4, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
            Assert.IsTrue(combinations[1] is ModA);
            Assert.IsTrue(combinations[2] is MultiMod);
            Assert.IsTrue(combinations[3] is ModB);

            Assert.IsTrue(((MultiMod)combinations[2]).Mods[0] is ModA);
            Assert.IsTrue(((MultiMod)combinations[2]).Mods[1] is ModB);
        }

        [Test]
        public void TestIncompatibleMods()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModIncompatibleWithA()).CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(3, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
            Assert.IsTrue(combinations[1] is ModA);
            Assert.IsTrue(combinations[2] is ModIncompatibleWithA);
        }

        [Test]
        public void TestDoubleIncompatibleMods()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModB(), new ModIncompatibleWithA(), new ModIncompatibleWithAAndB()).CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(8, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
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
            var combinations = new TestLegacyDifficultyCalculator(new ModAofA(), new ModIncompatibleWithAofA()).CreateDifficultyAdjustmentModCombinations();

            Assert.AreEqual(3, combinations.Length);
            Assert.IsTrue(combinations[0] is ModNoMod);
            Assert.IsTrue(combinations[1] is ModAofA);
            Assert.IsTrue(combinations[2] is ModIncompatibleWithAofA);
        }

        private class ModA : Mod
        {
            public override string Name => nameof(ModA);
            public override string Acronym => nameof(ModA);
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithA), typeof(ModIncompatibleWithAAndB) };
        }

        private class ModB : Mod
        {
            public override string Name => nameof(ModB);
            public override string Acronym => nameof(ModB);
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithAAndB) };
        }

        private class ModIncompatibleWithA : Mod
        {
            public override string Name => $"Incompatible With {nameof(ModA)}";
            public override string Acronym => $"Incompatible With {nameof(ModA)}";
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
            public override string Acronym => $"Incompatible With {nameof(ModA)} and {nameof(ModB)}";
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModA), typeof(ModB) };
        }

        private class TestLegacyDifficultyCalculator : DifficultyCalculator
        {
            public TestLegacyDifficultyCalculator(params Mod[] mods)
                : base(null, null)
            {
                DifficultyAdjustmentMods = mods;
            }

            protected override Mod[] DifficultyAdjustmentMods { get; }

            protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
            {
                throw new NotImplementedException();
            }

            protected override Skill[] CreateSkills(IBeatmap beatmap)
            {
                throw new NotImplementedException();
            }
        }
    }
}
