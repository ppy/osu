// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Extensions;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class BeatmapSetInfoEqualityTest
    {
        [Test]
        public void TestOnlineWithOnline()
        {
            var ourInfo = new BeatmapSetInfo { OnlineID = 123 };
            var otherInfo = new BeatmapSetInfo { OnlineID = 123 };

            Assert.AreNotEqual(ourInfo, otherInfo);
            Assert.IsTrue(ourInfo.MatchesOnlineID(otherInfo));
        }

        [Test]
        public void TestDatabasedWithDatabased()
        {
            var guid = Guid.NewGuid();

            var ourInfo = new BeatmapSetInfo { ID = guid };
            var otherInfo = new BeatmapSetInfo { ID = guid };

            Assert.AreEqual(ourInfo, otherInfo);
        }

        [Test]
        public void TestDatabasedWithOnline()
        {
            var ourInfo = new BeatmapSetInfo { ID = Guid.NewGuid(), OnlineID = 12 };
            var otherInfo = new BeatmapSetInfo { OnlineID = 12 };

            Assert.AreNotEqual(ourInfo, otherInfo);
            Assert.IsTrue(ourInfo.MatchesOnlineID(otherInfo));
        }

        [Test]
        public void TestCheckNullID()
        {
            var ourInfo = new BeatmapSetInfo { Hash = "1" };
            var otherInfo = new BeatmapSetInfo { Hash = "2" };

            Assert.AreNotEqual(ourInfo, otherInfo);
        }
    }
}
