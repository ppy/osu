// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Beatmaps;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    [TestFixture]
    public class ManiaKeyModMultiplierTest
    {
        /// <summary>
        /// Tests that key mods have no score multiplier penalty when applied to mania-specific beatmaps
        /// with the same key count.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void TestNativeKeyCountNoMultiplier(int keyCount)
        {
            var keyMod = createKeyMod(keyCount);
            var beatmap = createManiaBeatmap(keyCount);

            // Apply the mod to the beatmap converter
            var converter = new ManiaBeatmapConverter(beatmap, new ManiaRuleset());
            keyMod.ApplyToBeatmapConverter(converter);

            // For native key count, the multiplier should be 1.0 (no penalty)
            Assert.AreEqual(1.0, keyMod.ScoreMultiplier, $"{keyCount}K mod should have no penalty on native {keyCount}K beatmaps");
        }

        /// <summary>
        /// Tests that key mods have a score multiplier penalty when applied to mania-specific beatmaps
        /// with a different key count.
        /// </summary>
        [TestCase(4, 7)] // 4K mod on 7K map
        [TestCase(7, 4)] // 7K mod on 4K map
        [TestCase(5, 6)] // 5K mod on 6K map
        public void TestDifferentKeyCountHasMultiplier(int modKeyCount, int beatmapKeyCount)
        {
            var keyMod = createKeyMod(modKeyCount);
            var beatmap = createManiaBeatmap(beatmapKeyCount);

            // Apply the mod to the beatmap converter
            var converter = new ManiaBeatmapConverter(beatmap, new ManiaRuleset());
            keyMod.ApplyToBeatmapConverter(converter);

            // For different key count, the multiplier should be 0.9 (penalty)
            Assert.AreEqual(0.9, keyMod.ScoreMultiplier, $"{modKeyCount}K mod should have penalty on {beatmapKeyCount}K beatmaps");
        }

        /// <summary>
        /// Tests that key mods initially have a default penalty before being applied to any beatmap converter.
        /// </summary>
        [TestCase(4)]
        [TestCase(7)]
        public void TestDefaultMultiplierBeforeApplication(int keyCount)
        {
            var keyMod = createKeyMod(keyCount);

            // Before applying to any beatmap converter, the multiplier should default to penalty
            Assert.AreEqual(0.9, keyMod.ScoreMultiplier, $"{keyCount}K mod should have default penalty before application");
        }

        private ManiaKeyMod createKeyMod(int keyCount) => keyCount switch
        {
            1 => new ManiaModKey1(),
            2 => new ManiaModKey2(),
            3 => new ManiaModKey3(),
            4 => new ManiaModKey4(),
            5 => new ManiaModKey5(),
            6 => new ManiaModKey6(),
            7 => new ManiaModKey7(),
            8 => new ManiaModKey8(),
            9 => new ManiaModKey9(),
            10 => new ManiaModKey10(),
            _ => throw new System.ArgumentException($"Invalid key count: {keyCount}")
        };

        private IBeatmap createManiaBeatmap(int keyCount)
        {
            var beatmap = new TestBeatmap(new ManiaRuleset().RulesetInfo);
            // Set the circle size to match the key count for mania beatmaps
            beatmap.Difficulty.CircleSize = keyCount;
            return beatmap;
        }
    }
}
