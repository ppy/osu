// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Localisation
{
    [TestFixture]
    public class BeatmapMetadataRomanisationTest
    {
        [Test]
        public void TestNoUnicode()
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "Artist",
                        Title = "Romanised title"
                    }
                }
            };
            var romanisableString = beatmap.Metadata.ToRomanisableString();

            Assert.AreEqual(romanisableString.Romanised, romanisableString.Original);
        }
    }
}
