// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.IO.Archives;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps.IO;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Scores.IO
{
    public class ImportScoreTest : ImportTest
    {
        [Test]
        public async Task TestBasicImport()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    var beatmap = BeatmapImportHelper.LoadOszIntoOsu(osu, TestResources.GetQuickTestBeatmapForImport()).GetResultSafely();

                    var toImport = new ScoreInfo
                    {
                        Rank = ScoreRank.B,
                        TotalScore = 987654,
                        Accuracy = 0.8,
                        MaxCombo = 500,
                        Combo = 250,
                        User = new APIUser { Username = "Test user" },
                        Date = DateTimeOffset.Now,
                        OnlineID = 12345,
                        Ruleset = new OsuRuleset().RulesetInfo,
                        BeatmapInfo = beatmap.Beatmaps.First()
                    };

                    var imported = await LoadScoreIntoOsu(osu, toImport);

                    Assert.AreEqual(toImport.Rank, imported.Rank);
                    Assert.AreEqual(toImport.TotalScore, imported.TotalScore);
                    Assert.AreEqual(toImport.Accuracy, imported.Accuracy);
                    Assert.AreEqual(toImport.MaxCombo, imported.MaxCombo);
                    Assert.AreEqual(toImport.User.Username, imported.User.Username);
                    Assert.AreEqual(toImport.Date, imported.Date);
                    Assert.AreEqual(toImport.OnlineID, imported.OnlineID);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportMods()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    var beatmap = BeatmapImportHelper.LoadOszIntoOsu(osu, TestResources.GetQuickTestBeatmapForImport()).GetResultSafely();

                    var toImport = new ScoreInfo
                    {
                        User = new APIUser { Username = "Test user" },
                        BeatmapInfo = beatmap.Beatmaps.First(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                        Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
                    };

                    var imported = await LoadScoreIntoOsu(osu, toImport);

                    Assert.IsTrue(imported.Mods.Any(m => m is OsuModHardRock));
                    Assert.IsTrue(imported.Mods.Any(m => m is OsuModDoubleTime));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestImportStatistics()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    var beatmap = BeatmapImportHelper.LoadOszIntoOsu(osu, TestResources.GetQuickTestBeatmapForImport()).GetResultSafely();

                    var toImport = new ScoreInfo
                    {
                        User = new APIUser { Username = "Test user" },
                        BeatmapInfo = beatmap.Beatmaps.First(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                        Statistics = new Dictionary<HitResult, int>
                        {
                            { HitResult.Perfect, 100 },
                            { HitResult.Miss, 50 }
                        }
                    };

                    var imported = await LoadScoreIntoOsu(osu, toImport);

                    Assert.AreEqual(toImport.Statistics[HitResult.Perfect], imported.Statistics[HitResult.Perfect]);
                    Assert.AreEqual(toImport.Statistics[HitResult.Miss], imported.Statistics[HitResult.Miss]);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public async Task TestOnlineScoreIsAvailableLocally()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    var beatmap = BeatmapImportHelper.LoadOszIntoOsu(osu, TestResources.GetQuickTestBeatmapForImport()).GetResultSafely();

                    await LoadScoreIntoOsu(osu, new ScoreInfo
                    {
                        User = new APIUser { Username = "Test user" },
                        BeatmapInfo = beatmap.Beatmaps.First(),
                        Ruleset = new OsuRuleset().RulesetInfo,
                        OnlineID = 2
                    }, new TestArchiveReader());

                    var scoreManager = osu.Dependencies.Get<ScoreManager>();

                    // Note: A new score reference is used here since the import process mutates the original object to set an ID
                    Assert.That(scoreManager.IsAvailableLocally(new ScoreInfo
                    {
                        User = new APIUser { Username = "Test user" },
                        BeatmapInfo = beatmap.Beatmaps.First(),
                        OnlineID = 2
                    }));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        public static async Task<ScoreInfo> LoadScoreIntoOsu(OsuGameBase osu, ScoreInfo score, ArchiveReader archive = null)
        {
            // clone to avoid attaching the input score to realm.
            score = score.DeepClone();

            var scoreManager = osu.Dependencies.Get<ScoreManager>();
            await scoreManager.Import(score, archive);

            return scoreManager.Query(_ => true);
        }

        internal class TestArchiveReader : ArchiveReader
        {
            public TestArchiveReader()
                : base("test_archive")
            {
            }

            public override Stream GetStream(string name) => new MemoryStream();

            public override void Dispose()
            {
            }

            public override IEnumerable<string> Filenames => new[] { "test_file.osr" };
        }
    }
}
