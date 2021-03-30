// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class BeatmapDifficultyManagerTest
    {
        [Test]
        public void TestKeyEqualsWithDifferentModInstances()
        {
            var key1 = new BeatmapDifficultyCache.DifficultyCacheLookup(new BeatmapInfo { ID = 1234 }, new RulesetInfo { ID = 0 }, new Mod[] { new OsuModHardRock(), new OsuModHidden() });
            var key2 = new BeatmapDifficultyCache.DifficultyCacheLookup(new BeatmapInfo { ID = 1234 }, new RulesetInfo { ID = 0 }, new Mod[] { new OsuModHardRock(), new OsuModHidden() });

            Assert.That(key1, Is.EqualTo(key2));
        }

        [Test]
        public void TestKeyEqualsWithDifferentModOrder()
        {
            var key1 = new BeatmapDifficultyCache.DifficultyCacheLookup(new BeatmapInfo { ID = 1234 }, new RulesetInfo { ID = 0 }, new Mod[] { new OsuModHardRock(), new OsuModHidden() });
            var key2 = new BeatmapDifficultyCache.DifficultyCacheLookup(new BeatmapInfo { ID = 1234 }, new RulesetInfo { ID = 0 }, new Mod[] { new OsuModHidden(), new OsuModHardRock() });

            Assert.That(key1, Is.EqualTo(key2));
        }

        [TestCase(1.3, DifficultyRating.Easy)]
        [TestCase(1.993, DifficultyRating.Easy)]
        [TestCase(1.998, DifficultyRating.Normal)]
        [TestCase(2.4, DifficultyRating.Normal)]
        [TestCase(2.693, DifficultyRating.Normal)]
        [TestCase(2.698, DifficultyRating.Hard)]
        [TestCase(3.5, DifficultyRating.Hard)]
        [TestCase(3.993, DifficultyRating.Hard)]
        [TestCase(3.997, DifficultyRating.Insane)]
        [TestCase(5.0, DifficultyRating.Insane)]
        [TestCase(5.292, DifficultyRating.Insane)]
        [TestCase(5.297, DifficultyRating.Expert)]
        [TestCase(6.2, DifficultyRating.Expert)]
        [TestCase(6.493, DifficultyRating.Expert)]
        [TestCase(6.498, DifficultyRating.ExpertPlus)]
        [TestCase(8.3, DifficultyRating.ExpertPlus)]
        public void TestDifficultyRatingMapping(double starRating, DifficultyRating expectedBracket)
        {
            var actualBracket = BeatmapDifficultyCache.GetDifficultyRating(starRating);

            Assert.AreEqual(expectedBracket, actualBracket);
        }
    }
}
