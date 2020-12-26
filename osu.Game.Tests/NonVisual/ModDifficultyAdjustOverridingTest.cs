// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class ModDifficultyAdjustOverridingTest
    {
        private OsuModDifficultyAdjust mod;
        private int difficultyID;

        [SetUp]
        public void SetUp()
        {
            mod = new OsuModDifficultyAdjust();
            difficultyID = 0;
        }

        [Test]
        public void TestOverridenStopTrackingBase()
        {
            var baseDifficulty = createDifficulty(5);
            mod.ReadFromDifficulty(baseDifficulty);

            mod.ApproachRate.Value = 10;
            mod.CircleSize.Value = 10;

            Assert.AreEqual(10, mod.ApproachRate.Value);
            Assert.AreEqual(10, mod.CircleSize.Value);
            Assert.AreEqual(baseDifficulty.DrainRate, mod.DrainRate.Value);
            Assert.AreEqual(baseDifficulty.OverallDifficulty, mod.OverallDifficulty.Value);

            baseDifficulty = createDifficulty(3);
            mod.ReadFromDifficulty(baseDifficulty);

            Assert.AreEqual(10, mod.ApproachRate.Value);
            Assert.AreEqual(10, mod.CircleSize.Value);
            Assert.AreEqual(baseDifficulty.DrainRate, mod.DrainRate.Value);
            Assert.AreEqual(baseDifficulty.OverallDifficulty, mod.OverallDifficulty.Value);
        }

        [Test]
        public void TestOverridenAfterBaseTrackOnRevert()
        {
            var baseDifficulty = createDifficulty(5);
            mod.ReadFromDifficulty(baseDifficulty);

            mod.ApproachRate.Value = 10;
            Assert.AreEqual(10, mod.ApproachRate.Value);
            Assert.AreEqual(baseDifficulty.ApproachRate, mod.ApproachRate.Default);

            mod.ApproachRate.SetDefault();
            Assert.AreEqual(baseDifficulty.ApproachRate, mod.ApproachRate.Value);

            baseDifficulty = createDifficulty(3);
            mod.ReadFromDifficulty(baseDifficulty);

            Assert.AreEqual(baseDifficulty.ApproachRate, mod.ApproachRate.Value);
        }

        [Test]
        public void TestOverridenAfterBaseDontTrackWithoutChange()
        {
            mod.ReadFromDifficulty(createDifficulty(6));

            mod.ApproachRate.Value = 3;
            mod.CircleSize.Value = 3;

            Assert.AreEqual(3, mod.ApproachRate.Value);
            Assert.AreEqual(3, mod.CircleSize.Value);

            mod.ReadFromDifficulty(createDifficulty(3));

            Assert.AreEqual(3, mod.ApproachRate.Value);
            Assert.AreEqual(3, mod.CircleSize.Value);

            mod.ReadFromDifficulty(createDifficulty(4));

            Assert.AreEqual(3, mod.ApproachRate.Value);
            Assert.AreEqual(3, mod.CircleSize.Value);
        }

        [Test]
        public void TestOverrideBeforeBase()
        {
            mod.ApproachRate.Value = 10;
            mod.CircleSize.Value = 10;
            mod.DrainRate.Value = 10;
            mod.OverallDifficulty.Value = 10;

            mod.ReadFromDifficulty(createDifficulty(3));

            Assert.AreEqual(10, mod.ApproachRate.Value);
            Assert.AreEqual(10, mod.CircleSize.Value);
            Assert.AreEqual(10, mod.DrainRate.Value);
            Assert.AreEqual(10, mod.OverallDifficulty.Value);
        }

        [Test]
        public void TestOverridenBeforeBaseTracksBack()
        {
            mod.ApproachRate.Value = 6;
            mod.CircleSize.Value = 6;

            mod.ReadFromDifficulty(createDifficulty(6));

            Assert.AreEqual(6, mod.ApproachRate.Value);
            Assert.AreEqual(6, mod.CircleSize.Value);

            mod.ReadFromDifficulty(createDifficulty(3));

            Assert.AreEqual(3, mod.ApproachRate.Value);
            Assert.AreEqual(3, mod.CircleSize.Value);
        }

        private BeatmapDifficulty createDifficulty(float value) => new BeatmapDifficulty
        {
            ApproachRate = value,
            CircleSize = value,
            DrainRate = value,
            OverallDifficulty = value,
            ID = difficultyID++,
        };
    }
}
