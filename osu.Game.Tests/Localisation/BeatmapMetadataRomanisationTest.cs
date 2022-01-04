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
        public void TestRomanisation()
        {
            var metadata = new BeatmapMetadata
            {
                Artist = "Romanised Artist",
                ArtistUnicode = "Unicode Artist",
                Title = "Romanised title",
                TitleUnicode = "Unicode Title"
            };
            var romanisableString = metadata.GetDisplayTitleRomanisable();

            Assert.AreEqual(metadata.ToString(), romanisableString.Romanised);
            Assert.AreEqual($"{metadata.ArtistUnicode} - {metadata.TitleUnicode}", romanisableString.Original);
        }

        [Test]
        public void TestRomanisationNoUnicode()
        {
            var metadata = new BeatmapMetadata
            {
                Artist = "Romanised Artist",
                Title = "Romanised title"
            };
            var romanisableString = metadata.GetDisplayTitleRomanisable();

            Assert.AreEqual(romanisableString.Romanised, romanisableString.Original);
        }
    }
}
