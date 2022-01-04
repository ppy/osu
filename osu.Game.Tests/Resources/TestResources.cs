// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Tests.Resources
{
    public static class TestResources
    {
        public const double QUICK_BEATMAP_LENGTH = 10000;

        private static readonly TemporaryNativeStorage temp_storage = new TemporaryNativeStorage("TestResources");

        public static DllResourceStore GetStore() => new DllResourceStore(typeof(TestResources).Assembly);

        public static Stream OpenResource(string name) => GetStore().GetStream($"Resources/{name}");

        public static Stream GetTestBeatmapStream(bool virtualTrack = false) => OpenResource($"Archives/241526 Soleily - Renatus{(virtualTrack ? "_virtual" : "")}.osz");

        /// <summary>
        /// Retrieve a path to a copy of a shortened (~10 second) beatmap archive with a virtual track.
        /// </summary>
        /// <remarks>
        /// This is intended for use in tests which need to run to completion as soon as possible and don't need to test a full length beatmap.</remarks>
        /// <returns>A path to a copy of a beatmap archive (osz). Should be deleted after use.</returns>
        public static string GetQuickTestBeatmapForImport()
        {
            string tempPath = getTempFilename();
            using (var stream = OpenResource("Archives/241526 Soleily - Renatus_virtual_quick.osz"))
            using (var newFile = File.Create(tempPath))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(tempPath));
            return tempPath;
        }

        /// <summary>
        /// Retrieve a path to a copy of a full-fledged beatmap archive.
        /// </summary>
        /// <param name="virtualTrack">Whether the audio track should be virtual.</param>
        /// <returns>A path to a copy of a beatmap archive (osz). Should be deleted after use.</returns>
        public static string GetTestBeatmapForImport(bool virtualTrack = false)
        {
            string tempPath = getTempFilename();

            using (var stream = GetTestBeatmapStream(virtualTrack))
            using (var newFile = File.Create(tempPath))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(tempPath));
            return tempPath;
        }

        private static string getTempFilename() => temp_storage.GetFullPath(Guid.NewGuid() + ".osz");

        private static int importId;

        /// <summary>
        /// Create a test beatmap set model.
        /// </summary>
        /// <param name="difficultyCount">Number of difficulties. If null, a random number between 1 and 20 will be used.</param>
        /// <param name="rulesets">Rulesets to cycle through when creating difficulties. If <c>null</c>, osu! ruleset will be used.</param>
        public static BeatmapSetInfo CreateTestBeatmapSetInfo(int? difficultyCount = null, RulesetInfo[] rulesets = null)
        {
            int j = 0;
            RulesetInfo getRuleset() => rulesets?[j++ % rulesets.Length] ?? new OsuRuleset().RulesetInfo;

            int setId = Interlocked.Increment(ref importId);

            var metadata = new BeatmapMetadata
            {
                // Create random metadata, then we can check if sorting works based on these
                Artist = "Some Artist " + RNG.Next(0, 9),
                Title = $"Some Song (set id {setId}) {Guid.NewGuid()}",
                AuthorString = "Some Guy " + RNG.Next(0, 9),
            };

            var beatmapSet = new BeatmapSetInfo
            {
                OnlineID = setId,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                DateAdded = DateTimeOffset.UtcNow,
                Metadata = metadata
            };

            foreach (var b in getBeatmaps(difficultyCount ?? RNG.Next(1, 20)))
                beatmapSet.Beatmaps.Add(b);

            return beatmapSet;

            IEnumerable<BeatmapInfo> getBeatmaps(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    int beatmapId = setId * 1000 + i;

                    int length = RNG.Next(30000, 200000);
                    double bpm = RNG.NextSingle(80, 200);

                    float diff = (float)i / count * 10;

                    string version = "Normal";
                    if (diff > 6.6)
                        version = "Insane";
                    else if (diff > 3.3)
                        version = "Hard";

                    var rulesetInfo = getRuleset();

                    yield return new BeatmapInfo
                    {
                        OnlineID = beatmapId,
                        DifficultyName = $"{version} {beatmapId} (length {TimeSpan.FromMilliseconds(length):m\\:ss}, bpm {bpm:0.#})",
                        StarRating = diff,
                        Length = length,
                        BPM = bpm,
                        Ruleset = rulesetInfo,
                        RulesetID = rulesetInfo.ID ?? -1,
                        Metadata = metadata,
                        BaseDifficulty = new BeatmapDifficulty
                        {
                            OverallDifficulty = diff,
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Create a test score model.
        /// </summary>
        /// <param name="ruleset">The ruleset for which the score was set against.</param>
        /// <returns></returns>
        public static ScoreInfo CreateTestScoreInfo(RulesetInfo ruleset = null) =>
            CreateTestScoreInfo(CreateTestBeatmapSetInfo(1, new[] { ruleset ?? new OsuRuleset().RulesetInfo }).Beatmaps.First());

        /// <summary>
        /// Create a test score model.
        /// </summary>
        /// <param name="beatmap">The beatmap for which the score was set against.</param>
        /// <returns></returns>
        public static ScoreInfo CreateTestScoreInfo(BeatmapInfo beatmap) => new ScoreInfo
        {
            User = new APIUser
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            },
            BeatmapInfo = beatmap,
            Ruleset = beatmap.Ruleset,
            RulesetID = beatmap.Ruleset.ID ?? 0,
            Mods = new Mod[] { new TestModHardRock(), new TestModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = 0.95,
            MaxCombo = 999,
            Position = 1,
            Rank = ScoreRank.S,
            Date = DateTimeOffset.Now,
            Statistics = new Dictionary<HitResult, int>
            {
                [HitResult.Miss] = 1,
                [HitResult.Meh] = 50,
                [HitResult.Ok] = 100,
                [HitResult.Good] = 200,
                [HitResult.Great] = 300,
                [HitResult.Perfect] = 320,
                [HitResult.SmallTickHit] = 50,
                [HitResult.SmallTickMiss] = 25,
                [HitResult.LargeTickHit] = 100,
                [HitResult.LargeTickMiss] = 50,
                [HitResult.SmallBonus] = 10,
                [HitResult.SmallBonus] = 50
            },
        };

        private class TestModHardRock : ModHardRock
        {
            public override double ScoreMultiplier => 1;
        }

        private class TestModDoubleTime : ModDoubleTime
        {
            public override double ScoreMultiplier => 1;
        }
    }
}
