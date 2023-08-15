// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Tests.Models
{
    [TestFixture]
    public class DisplayStringTest
    {
        [Test]
        public void TestNull()
        {
            IBeatmapSetInfo? beatmap = null;
            Assert.That(beatmap.GetDisplayString(), Is.EqualTo("null"));
        }

        [Test]
        public void TestBeatmapSet()
        {
            var mock = new Mock<IBeatmapSetInfo>();

            mock.Setup(m => m.Metadata.Artist).Returns("artist");
            mock.Setup(m => m.Metadata.Title).Returns("title");
            mock.Setup(m => m.Metadata.Author.Username).Returns("author");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("artist - title (author)"));
        }

        [Test]
        public void TestBeatmapSetWithNoAuthor()
        {
            var mock = new Mock<IBeatmapSetInfo>();

            mock.Setup(m => m.Metadata.Artist).Returns("artist");
            mock.Setup(m => m.Metadata.Title).Returns("title");
            mock.Setup(m => m.Metadata.Author.Username).Returns(string.Empty);

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("artist - title"));
        }

        [Test]
        public void TestBeatmapSetWithNoMetadata()
        {
            var mock = new Mock<IBeatmapSetInfo>();

            mock.Setup(m => m.Metadata).Returns(new BeatmapMetadata());

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("unknown artist - unknown title"));
        }

        [Test]
        public void TestBeatmap()
        {
            var mock = new Mock<IBeatmapInfo>();

            mock.Setup(m => m.Metadata.Artist).Returns("artist");
            mock.Setup(m => m.Metadata.Title).Returns("title");
            mock.Setup(m => m.Metadata.Author.Username).Returns("author");
            mock.Setup(m => m.DifficultyName).Returns("difficulty");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("artist - title (author) [difficulty]"));
        }

        [Test]
        public void TestMetadata()
        {
            var mock = new Mock<IBeatmapMetadataInfo>();

            mock.Setup(m => m.Artist).Returns("artist");
            mock.Setup(m => m.Title).Returns("title");
            mock.Setup(m => m.Author.Username).Returns("author");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("artist - title (author)"));
        }

        [Test]
        public void TestScore()
        {
            var mock = new Mock<IScoreInfo>();

            mock.Setup(m => m.User).Returns(new APIUser { Username = "user" }); // TODO: temporary.
            mock.Setup(m => m.Beatmap!.Metadata.Artist).Returns("artist");
            mock.Setup(m => m.Beatmap!.Metadata.Title).Returns("title");
            mock.Setup(m => m.Beatmap!.Metadata.Author.Username).Returns("author");
            mock.Setup(m => m.Beatmap!.DifficultyName).Returns("difficulty");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("user playing artist - title (author) [difficulty]"));
        }

        [Test]
        public void TestRuleset()
        {
            var mock = new Mock<IRulesetInfo>();

            mock.Setup(m => m.Name).Returns("ruleset");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("ruleset"));
        }

        [Test]
        public void TestUser()
        {
            var mock = new Mock<IUser>();

            mock.Setup(m => m.Username).Returns("user");

            Assert.That(mock.Object.GetDisplayString(), Is.EqualTo("user"));
        }

        [Test]
        public void TestFallback()
        {
            var fallback = new Fallback();

            Assert.That(fallback.GetDisplayString(), Is.EqualTo("fallback"));
        }

        private class Fallback
        {
            public override string ToString() => "fallback";
        }
    }
}
