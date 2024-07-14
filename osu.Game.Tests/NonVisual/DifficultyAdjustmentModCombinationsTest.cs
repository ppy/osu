// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class DifficultyAdjustmentModCombinationsTest
    {
        [Test]
        public void TestNoMods()
        {
            var combinations = new TestLegacyDifficultyCalculator().CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) }
            }, combinations);
        }

        [Test]
        public void TestSingleMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA()).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) }
            }, combinations);
        }

        [Test]
        public void TestDoubleMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModB()).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModA), typeof(ModB) },
                new[] { typeof(ModB) }
            }, combinations);
        }

        [Test]
        public void TestIncompatibleMods()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModIncompatibleWithA()).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModIncompatibleWithA) }
            }, combinations);
        }

        [Test]
        public void TestDoubleIncompatibleMods()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new ModB(), new ModIncompatibleWithA(), new ModIncompatibleWithAAndB()).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModA), typeof(ModB) },
                new[] { typeof(ModB) },
                new[] { typeof(ModB), typeof(ModIncompatibleWithA) },
                new[] { typeof(ModIncompatibleWithA) },
                new[] { typeof(ModIncompatibleWithA), typeof(ModIncompatibleWithAAndB) },
                new[] { typeof(ModIncompatibleWithAAndB) },
            }, combinations);
        }

        [Test]
        public void TestIncompatibleThroughBaseType()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModAofA(), new ModIncompatibleWithAofA()).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModAofA) },
                new[] { typeof(ModIncompatibleWithAofA) }
            }, combinations);
        }

        [Test]
        public void TestMultiModFlattening()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new MultiMod(new ModB(), new ModC())).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModA), typeof(ModB), typeof(ModC) },
                new[] { typeof(ModB), typeof(ModC) }
            }, combinations);
        }

        [Test]
        public void TestIncompatibleThroughMultiMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new MultiMod(new ModB(), new ModIncompatibleWithA())).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModB), typeof(ModIncompatibleWithA) }
            }, combinations);
        }

        [Test]
        public void TestIncompatibleWithSameInstanceViaMultiMod()
        {
            var combinations = new TestLegacyDifficultyCalculator(new ModA(), new MultiMod(new ModA(), new ModB())).CreateDifficultyAdjustmentModCombinations();

            assertCombinations(new[]
            {
                new[] { typeof(ModNoMod) },
                new[] { typeof(ModA) },
                new[] { typeof(ModA), typeof(ModB) }
            }, combinations);
        }

        private void assertCombinations(Type[][] expectedCombinations, Mod[] actualCombinations)
        {
            Assert.AreEqual(expectedCombinations.Length, actualCombinations.Length);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < expectedCombinations.Length; ++i)
                {
                    Type[] expectedTypes = expectedCombinations[i];
                    Type[] actualTypes = ModUtils.FlattenMod(actualCombinations[i]).Select(m => m.GetType()).ToArray();

                    Assert.That(expectedTypes, Is.EquivalentTo(actualTypes));
                }
            });
        }

        private class ModA : Mod
        {
            public override string Name => nameof(ModA);
            public override string Acronym => nameof(ModA);
            public override LocalisableString Description => string.Empty;
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithA), typeof(ModIncompatibleWithAAndB) };
        }

        private class ModB : Mod
        {
            public override string Name => nameof(ModB);
            public override LocalisableString Description => string.Empty;
            public override string Acronym => nameof(ModB);
            public override double ScoreMultiplier => 1;

            public override Type[] IncompatibleMods => new[] { typeof(ModIncompatibleWithAAndB) };
        }

        private class ModC : Mod
        {
            public override string Name => nameof(ModC);
            public override string Acronym => nameof(ModC);
            public override LocalisableString Description => string.Empty;
            public override double ScoreMultiplier => 1;
        }

        private class ModIncompatibleWithA : Mod
        {
            public override string Name => $"Incompatible With {nameof(ModA)}";
            public override string Acronym => $"Incompatible With {nameof(ModA)}";
            public override LocalisableString Description => string.Empty;
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
            public override LocalisableString Description => string.Empty;
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

            protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
            {
                throw new NotImplementedException();
            }
        }
    }
}
