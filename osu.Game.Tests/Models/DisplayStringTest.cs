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
        private static readonly object[][] test_cases =
        {
            new object[] { makeMockBeatmapSet(), "artist - title (author)" },
            new object[] { makeMockBeatmap(), "artist - title (author) [difficulty]" },
            new object[] { makeMockMetadata(), "artist - title (author)" },
            new object[] { makeMockScore(), "user playing artist - title (author) [difficulty]" },
            new object[] { makeMockRuleset(), "ruleset" },
            new object[] { makeMockUser(), "user" },
            new object[] { new Fallback(), "fallback" }
        };

        [TestCaseSource(nameof(test_cases))]
        public void TestDisplayString(object model, string expected) => Assert.That(model.GetDisplayString(), Is.EqualTo(expected));

        private static IBeatmapSetInfo makeMockBeatmapSet()
        {
            var mock = new Mock<IBeatmapSetInfo>();

            mock.Setup(m => m.Metadata).Returns(makeMockMetadata);

            return mock.Object;
        }

        private static IBeatmapInfo makeMockBeatmap()
        {
            var mock = new Mock<IBeatmapInfo>();

            mock.Setup(m => m.Metadata).Returns(makeMockMetadata);
            mock.Setup(m => m.DifficultyName).Returns("difficulty");

            return mock.Object;
        }

        private static IBeatmapMetadataInfo makeMockMetadata()
        {
            var mock = new Mock<IBeatmapMetadataInfo>();

            mock.Setup(m => m.Artist).Returns("artist");
            mock.Setup(m => m.Title).Returns("title");
            mock.Setup(m => m.Author.Username).Returns("author");

            return mock.Object;
        }

        private static IScoreInfo makeMockScore()
        {
            var mock = new Mock<IScoreInfo>();

            mock.Setup(m => m.User).Returns(new APIUser { Username = "user" }); // TODO: temporary.
            mock.Setup(m => m.Beatmap).Returns(makeMockBeatmap);

            return mock.Object;
        }

        private static IRulesetInfo makeMockRuleset()
        {
            var mock = new Mock<IRulesetInfo>();

            mock.Setup(m => m.Name).Returns("ruleset");

            return mock.Object;
        }

        private static IUser makeMockUser()
        {
            var mock = new Mock<IUser>();

            mock.Setup(m => m.Username).Returns("user");

            return mock.Object;
        }

        private class Fallback
        {
            public override string ToString() => "fallback";
        }
    }
}
