// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Resources;
using osu.Game.Users;

namespace osu.Game.Tests.Scores.IO
{
    public class ImportScoreTest
    {
        [Test]
        public void TestBasicImport()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestBasicImport"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var toImport = new ScoreInfo
                    {
                        Rank = ScoreRank.B,
                        TotalScore = 987654,
                        Accuracy = 0.8,
                        MaxCombo = 500,
                        Combo = 250,
                        User = new User { Username = "Test user" },
                        Date = DateTimeOffset.Now,
                        OnlineScoreID = 12345,
                    };

                    var imported = loadIntoOsu(osu, toImport);

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
        public void TestImportMods()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportMods"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var toImport = new ScoreInfo
                    {
                        Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
                    };

                    var imported = loadIntoOsu(osu, toImport);

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
        public void TestImportStatistics()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost("TestImportStatistics"))
            {
                try
                {
                    var osu = loadOsu(host);

                    var toImport = new ScoreInfo
                    {
                        Statistics = new Dictionary<HitResult, int>
                        {
                            { HitResult.Perfect, 100 },
                            { HitResult.Miss, 50 }
                        }
                    };

                    var imported = loadIntoOsu(osu, toImport);

                    Assert.AreEqual(toImport.Statistics[HitResult.Perfect], imported.Statistics[HitResult.Perfect]);
                    Assert.AreEqual(toImport.Statistics[HitResult.Miss], imported.Statistics[HitResult.Miss]);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        private ScoreInfo loadIntoOsu(OsuGameBase osu, ScoreInfo score)
        {
            var beatmapManager = osu.Dependencies.Get<BeatmapManager>();

            score.Beatmap = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();
            score.Ruleset = new OsuRuleset().RulesetInfo;

            var scoreManager = osu.Dependencies.Get<ScoreManager>();
            scoreManager.Import(score);

            return scoreManager.GetAllUsableScores().First();
        }

        private OsuGameBase loadOsu(GameHost host)
        {
            var osu = new OsuGameBase();
            Task.Run(() => host.Run(osu));
            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");

            var beatmapFile = TestResources.GetTestBeatmapForImport();
            var beatmapManager = osu.Dependencies.Get<BeatmapManager>();
            beatmapManager.Import(beatmapFile);

            return osu;
        }

        private void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Run(() =>
            {
                while (!result()) Thread.Sleep(200);
            });

            Assert.IsTrue(task.Wait(timeout), failureMessage);
        }
    }
}
