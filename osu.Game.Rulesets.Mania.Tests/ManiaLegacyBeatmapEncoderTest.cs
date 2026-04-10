// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.IO.Stores;
using static osu.Game.Tests.Beatmaps.Formats.LegacyBeatmapEncoderTest;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaLegacyBeatmapEncoderTest
    {
        private static readonly DllResourceStore beatmaps_resource_store = new DllResourceStore(typeof(ManiaLegacyBeatmapEncoderTest).Assembly);

        [TestCase("1K")]
        [TestCase("2K")]
        [TestCase("3K")]
        [TestCase("4K")]
        [TestCase("5K")]
        [TestCase("6K")]
        [TestCase("7K")]
        [TestCase("8K")]
        [TestCase("9K")]
        [TestCase("10K")]
        // [TestCase("11K")] <- See comment in `ManiaBeatmapConverter` ctor for disable reason.
        [TestCase("12K")]
        // [TestCase("13K")] <- See comment in `ManiaBeatmapConverter` ctor for disable reason.
        [TestCase("14K")]
        // [TestCase("15K")] <- See comment in `ManiaBeatmapConverter` ctor for disable reason.
        [TestCase("16K")]
        // [TestCase("17K")] <- See comment in `ManiaBeatmapConverter` ctor for disable reason.
        [TestCase("18K")]
        [TestCase("7K+1")]
        public void TestEncodeDecodeStability(string name)
        {
            var decoded = DecodeFromLegacy(beatmaps_resource_store.GetStream($"Resources/Testing/Beatmaps/{name}.osu"), beatmaps_resource_store, name);
            var decodedAfterEncode = DecodeFromLegacy(EncodeToLegacy(decoded), beatmaps_resource_store, name);

            Sort(decoded.beatmap);
            Sort(decodedAfterEncode.beatmap);

            CompareBeatmaps(decoded, decodedAfterEncode);
        }
    }
}
