// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.IO.Archives;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

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

                    var toImport = new ScoreInfo
                    {
                        Rank = ScoreRank.B,
                        TotalScore = 987654,
                        Accuracy = 0.8,
                        MaxCombo = 500,
                        Combo = 250,
                        User = new APIUser { Username = "Test user" },
                        Date = DateTimeOffset.Now,
                        OnlineScoreID = 12345,
                    };

                    var imported = await LoadScoreIntoOsu(osu, toImport);

                    Assert.AreEqual(toImport.Rank, imported.Rank);
                    Assert.AreEqual(toImport.TotalScore, imported.TotalScore);
                    Assert.AreEqual(toImport.Accuracy, imported.Accuracy);
                    Assert.AreEqual(toImport.MaxCombo, imported.MaxCombo);
                    Assert.AreEqual(toImport.Combo, imported.Combo);
                    Assert.AreEqual(toImport.User.Username, imported.User.Username);
                    Assert.AreEqual(toImport.Date, imported.Date);
                    Assert.AreEqual(toImport.OnlineScoreID, imported.OnlineScoreID);
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

                    var toImport = new ScoreInfo
                    {
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

                    var toImport = new ScoreInfo
                    {
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
        public async Task TestImportWithDeletedBeatmapSet()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = LoadOsuIntoHost(host, true);

                    var toImport = new ScoreInfo
                    {
                        Hash = Guid.NewGuid().ToString(),
                        Statistics = new Dictionary<HitResult, int>
                        {
                            { HitResult.Perfect, 100 },
                            { HitResult.Miss, 50 }
                        }
                    };

                    var imported = await LoadScoreIntoOsu(osu, toImport);

                    var beatmapManager = osu.Dependencies.Get<BeatmapManager>();
                    var scoreManager = osu.Dependencies.Get<ScoreManager>();

                    beatmapManager.Delete(beatmapManager.QueryBeatmapSet(s => s.Beatmaps.Any(b => b.ID == imported.BeatmapInfo.ID)));
                    Assert.That(scoreManager.Query(s => s.ID == imported.ID).DeletePending, Is.EqualTo(true));

                    var secondImport = await LoadScoreIntoOsu(osu, imported);
                    Assert.That(secondImport, Is.Null);
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

                    await LoadScoreIntoOsu(osu, new ScoreInfo { OnlineScoreID = 2 }, new TestArchiveReader());

                    var scoreManager = osu.Dependencies.Get<ScoreManager>();

                    // Note: A new score reference is used here since the import process mutates the original object to set an ID
                    Assert.That(scoreManager.IsAvailableLocally(new ScoreInfo { OnlineScoreID = 2 }));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        public static async Task<ScoreInfo> LoadScoreIntoOsu(OsuGameBase osu, ScoreInfo score, ArchiveReader archive = null)
        {
            var beatmapManager = osu.Dependencies.Get<BeatmapManager>();

            score.BeatmapInfo ??= beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();
            score.Ruleset ??= new OsuRuleset().RulesetInfo;

            var scoreManager = osu.Dependencies.Get<ScoreManager>();
            await scoreManager.Import(score, archive);

            return scoreManager.GetAllUsableScores().FirstOrDefault();
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
