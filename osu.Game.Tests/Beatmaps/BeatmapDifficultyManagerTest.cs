// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
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
            var key1 = new BeatmapDifficultyManager.DifficultyCacheLookup(1234, 0, new Mod[] { new OsuModHardRock(), new OsuModHidden() });
            var key2 = new BeatmapDifficultyManager.DifficultyCacheLookup(1234, 0, new Mod[] { new OsuModHardRock(), new OsuModHidden() });

            Assert.That(key1, Is.EqualTo(key2));
        }

        [Test]
        public void TestKeyEqualsWithDifferentModOrder()
        {
            var key1 = new BeatmapDifficultyManager.DifficultyCacheLookup(1234, 0, new Mod[] { new OsuModHardRock(), new OsuModHidden() });
            var key2 = new BeatmapDifficultyManager.DifficultyCacheLookup(1234, 0, new Mod[] { new OsuModHidden(), new OsuModHardRock() });

            Assert.That(key1, Is.EqualTo(key2));
        }
    }
}
