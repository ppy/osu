// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Tests.Resources;

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
        public void TestAudioEqualityNoFile()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            Assert.AreNotEqual(beatmapSetA, beatmapSetB);
            Assert.IsTrue(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualitySameHash()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            addAudioFile(beatmapSetA, "abc");
            addAudioFile(beatmapSetB, "abc");

            Assert.AreNotEqual(beatmapSetA, beatmapSetB);
            Assert.IsTrue(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualityDifferentHash()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            addAudioFile(beatmapSetA);
            addAudioFile(beatmapSetB);

            Assert.AreNotEqual(beatmapSetA, beatmapSetB);
            Assert.IsTrue(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualityBeatmapInfoSameHash()
        {
            var beatmapSet = TestResources.CreateTestBeatmapSetInfo(2);

            addAudioFile(beatmapSet);

            var beatmap1 = beatmapSet.Beatmaps.First();
            var beatmap2 = beatmapSet.Beatmaps.Last();

            Assert.AreNotEqual(beatmap1, beatmap2);
            Assert.IsTrue(beatmap1.AudioEquals(beatmap2));
        }

        [Test]
        public void TestAudioEqualityBeatmapInfoDifferentHash()
        {
            var beatmapSet = TestResources.CreateTestBeatmapSetInfo(2);

            const string filename1 = "audio1.mp3";
            const string filename2 = "audio2.mp3";

            addAudioFile(beatmapSet, filename: filename1);
            addAudioFile(beatmapSet, filename: filename2);

            var beatmap1 = beatmapSet.Beatmaps.First();
            var beatmap2 = beatmapSet.Beatmaps.Last();

            Assert.AreNotEqual(beatmap1, beatmap2);

            beatmap1.Metadata.AudioFile = filename1;
            beatmap2.Metadata.AudioFile = filename2;

            Assert.IsFalse(beatmap1.AudioEquals(beatmap2));
        }

        private static void addAudioFile(BeatmapSetInfo beatmapSetInfo, string hash = null, string filename = null)
        {
            beatmapSetInfo.Files.Add(new RealmNamedFileUsage(new RealmFile { Hash = hash ?? Guid.NewGuid().ToString() }, filename ?? "audio.mp3"));
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
