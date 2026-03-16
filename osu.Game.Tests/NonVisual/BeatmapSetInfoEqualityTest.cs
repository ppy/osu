// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

            ClassicAssert.AreNotEqual(ourInfo, otherInfo);
            ClassicAssert.True(ourInfo.MatchesOnlineID(otherInfo));
        }

        [Test]
        public void TestAudioEqualityNoFile()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            ClassicAssert.AreNotEqual(beatmapSetA, beatmapSetB);
            ClassicAssert.True(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualityCaseSensitivity()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            // empty by default so let's set it..
            beatmapSetA.Beatmaps.First().Metadata.AudioFile = "audio.mp3";
            beatmapSetB.Beatmaps.First().Metadata.AudioFile = "audio.mp3";

            addAudioFile(beatmapSetA, "abc", "AuDiO.mP3");
            addAudioFile(beatmapSetB, "abc", "audio.mp3");

            ClassicAssert.AreNotEqual(beatmapSetA, beatmapSetB);
            ClassicAssert.True(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualitySameHash()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            addAudioFile(beatmapSetA, "abc");
            addAudioFile(beatmapSetB, "abc");

            ClassicAssert.AreNotEqual(beatmapSetA, beatmapSetB);
            ClassicAssert.True(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualityDifferentHash()
        {
            var beatmapSetA = TestResources.CreateTestBeatmapSetInfo(1);
            var beatmapSetB = TestResources.CreateTestBeatmapSetInfo(1);

            addAudioFile(beatmapSetA);
            addAudioFile(beatmapSetB);

            ClassicAssert.AreNotEqual(beatmapSetA, beatmapSetB);
            ClassicAssert.True(beatmapSetA.Beatmaps.Single().AudioEquals(beatmapSetB.Beatmaps.Single()));
        }

        [Test]
        public void TestAudioEqualityBeatmapInfoSameHash()
        {
            var beatmapSet = TestResources.CreateTestBeatmapSetInfo(2);

            addAudioFile(beatmapSet);

            var beatmap1 = beatmapSet.Beatmaps.First();
            var beatmap2 = beatmapSet.Beatmaps.Last();

            ClassicAssert.AreNotEqual(beatmap1, beatmap2);
            ClassicAssert.True(beatmap1.AudioEquals(beatmap2));
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

            ClassicAssert.AreNotEqual(beatmap1, beatmap2);

            beatmap1.Metadata.AudioFile = filename1;
            beatmap2.Metadata.AudioFile = filename2;

            ClassicAssert.False(beatmap1.AudioEquals(beatmap2));
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

            ClassicAssert.AreEqual(ourInfo, otherInfo);
        }

        [Test]
        public void TestDatabasedWithOnline()
        {
            var ourInfo = new BeatmapSetInfo { ID = Guid.NewGuid(), OnlineID = 12 };
            var otherInfo = new BeatmapSetInfo { OnlineID = 12 };

            ClassicAssert.AreNotEqual(ourInfo, otherInfo);
            ClassicAssert.True(ourInfo.MatchesOnlineID(otherInfo));
        }

        [Test]
        public void TestCheckNullID()
        {
            var ourInfo = new BeatmapSetInfo { Hash = "1" };
            var otherInfo = new BeatmapSetInfo { Hash = "2" };

            ClassicAssert.AreNotEqual(ourInfo, otherInfo);
        }
    }
}
