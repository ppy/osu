// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class ModDifficultyAdjustTest
    {
        private TestModDifficultyAdjust testMod;

        [SetUp]
        public void Setup()
        {
            testMod = new TestModDifficultyAdjust();
        }

        [Test]
        public void TestUnchangedSettingsFollowAppliedDifficulty()
        {
            var result = applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 10,
                OverallDifficulty = 10
            });

            Assert.That(result.DrainRate, Is.EqualTo(10));
            Assert.That(result.OverallDifficulty, Is.EqualTo(10));

            result = applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 1,
                OverallDifficulty = 1
            });

            Assert.That(result.DrainRate, Is.EqualTo(1));
            Assert.That(result.OverallDifficulty, Is.EqualTo(1));
        }

        [Test]
        public void TestChangedSettingsOverrideAppliedDifficulty()
        {
            testMod.OverallDifficulty.Value = 4;

            var result = applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 10,
                OverallDifficulty = 10
            });

            Assert.That(result.DrainRate, Is.EqualTo(10));
            Assert.That(result.OverallDifficulty, Is.EqualTo(4));

            result = applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 1,
                OverallDifficulty = 1
            });

            Assert.That(result.DrainRate, Is.EqualTo(1));
            Assert.That(result.OverallDifficulty, Is.EqualTo(4));
        }

        [Test]
        public void TestChangedSettingsRetainedWhenSameValueIsApplied()
        {
            testMod.OverallDifficulty.Value = 4;

            // Apply and de-apply the same value as the mod.
            applyDifficulty(new BeatmapDifficulty { OverallDifficulty = 4 });
            var result = applyDifficulty(new BeatmapDifficulty { OverallDifficulty = 10 });

            Assert.That(result.OverallDifficulty, Is.EqualTo(4));
        }

        [Test]
        public void TestChangedSettingSerialisedWhenSameValueIsApplied()
        {
            applyDifficulty(new BeatmapDifficulty { OverallDifficulty = 4 });
            testMod.OverallDifficulty.Value = 4;

            var result = (TestModDifficultyAdjust)new APIMod(testMod).ToMod(new TestRuleset());

            Assert.That(result.OverallDifficulty.Value, Is.EqualTo(4));
        }

        [Test]
        public void TestChangedSettingsRevertedToDefault()
        {
            applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 10,
                OverallDifficulty = 10
            });

            testMod.OverallDifficulty.Value = 4;
            testMod.ResetSettingsToDefaults();

            Assert.That(testMod.DrainRate.Value, Is.Null);

            // ReSharper disable once HeuristicUnreachableCode
            // see https://youtrack.jetbrains.com/issue/RIDER-70159.
            Assert.That(testMod.OverallDifficulty.Value, Is.Null);

            var applied = applyDifficulty(new BeatmapDifficulty
            {
                DrainRate = 10,
                OverallDifficulty = 10
            });

            Assert.That(applied.OverallDifficulty, Is.EqualTo(10));
        }

        /// <summary>
        /// Applies a <see cref="BeatmapDifficulty"/> to the mod and returns a new <see cref="BeatmapDifficulty"/>
        /// representing the result if the mod were applied to a fresh <see cref="BeatmapDifficulty"/> instance.
        /// </summary>
        private BeatmapDifficulty applyDifficulty(BeatmapDifficulty difficulty)
        {
            // ensure that ReadFromDifficulty doesn't pollute the values.
            var newDifficulty = difficulty.Clone();

            testMod.ReadFromDifficulty(difficulty);

            testMod.ApplyToDifficulty(newDifficulty);
            return newDifficulty;
        }

        private class TestModDifficultyAdjust : ModDifficultyAdjust
        {
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                if (type == ModType.DifficultyIncrease)
                    yield return new TestModDifficultyAdjust();
            }

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            {
                throw new System.NotImplementedException();
            }

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
            {
                throw new System.NotImplementedException();
            }

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
            {
                throw new System.NotImplementedException();
            }

            public override string Description => string.Empty;
            public override string ShortName => string.Empty;
        }
    }
}
