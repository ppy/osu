// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class ToStringFormattingTest
    {
        [Test]
        public void TestArtistTitle()
        {
            var beatmap = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "artist",
                    Title = "title"
                }
            };

            Assert.That(beatmap.ToString(), Is.EqualTo("artist - title"));
        }

        [Test]
        public void TestArtistTitleCreator()
        {
            var beatmap = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "artist",
                    Title = "title",
                    Author = new APIUser { Username = "creator" }
                }
            };

            Assert.That(beatmap.ToString(), Is.EqualTo("artist - title (creator)"));
        }

        [Test]
        public void TestArtistTitleCreatorDifficulty()
        {
            var beatmap = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "artist",
                    Title = "title",
                    Author = new APIUser { Username = "creator" }
                },
                Version = "difficulty"
            };

            Assert.That(beatmap.ToString(), Is.EqualTo("artist - title (creator) [difficulty]"));
        }
    }
}
