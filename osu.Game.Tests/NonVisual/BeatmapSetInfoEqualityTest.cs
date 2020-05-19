// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class BeatmapSetInfoEqualityTest
    {
        [Test]
        public void TestOnlineWithOnline()
        {
            var ourInfo = new BeatmapSetInfo { OnlineBeatmapSetID = 123 };
            var otherInfo = new BeatmapSetInfo { OnlineBeatmapSetID = 123 };

            Assert.AreEqual(ourInfo, otherInfo);
        }

        [Test]
        public void TestDatabasedWithDatabased()
        {
            var ourInfo = new BeatmapSetInfo { ID = 123 };
            var otherInfo = new BeatmapSetInfo { ID = 123 };

            Assert.AreEqual(ourInfo, otherInfo);
        }

        [Test]
        public void TestDatabasedWithOnline()
        {
            var ourInfo = new BeatmapSetInfo { ID = 123, OnlineBeatmapSetID = 12 };
            var otherInfo = new BeatmapSetInfo { OnlineBeatmapSetID = 12 };

            Assert.AreEqual(ourInfo, otherInfo);
        }

        [Test]
        public void TestCheckNullID()
        {
            var ourInfo = new BeatmapSetInfo { Status = BeatmapSetOnlineStatus.Loved };
            var otherInfo = new BeatmapSetInfo { Status = BeatmapSetOnlineStatus.Approved };

            Assert.AreNotEqual(ourInfo, otherInfo);
        }
    }
}
