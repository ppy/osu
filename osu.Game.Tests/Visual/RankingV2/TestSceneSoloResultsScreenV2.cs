// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Ranking;
using Realms;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneSoloResultsScreenV2 : ScreenTestScene
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private int onlineScoreID = 1;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.Run(r =>
            {
                var beatmapInfo = r.All<BeatmapInfo>()
                                   .Filter($"{nameof(BeatmapInfo.Ruleset)}.{nameof(RulesetInfo.OnlineID)} = $0 AND {nameof(BeatmapInfo.StatusInt)} = 1", 0)
                                   .FirstOrDefault();

                if (beatmapInfo != null)
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
            });
        }

        [Test]
        public void TestDisplay()
        {
            SoloResultsScreenV2 soloResults = null!;
            SkinEditorOverlay skinEditor = null!;

            AddStep("create screen", () =>
            {
                var score = createTestScore();
                LoadScreen(soloResults = new SoloResultsScreenV2(score));
                // dodgy as hell but it's annoying to set this up otherwise.
                // TL;DR: the intention is to add the skin editor *outside* of the scaling container
                // but tricks like `base.Content.Add()` don't work here because that is *inside* the scaling container already
                // which makes the skin editor look broken
                ((Container)ScalingContainer.Parent!).Add(skinEditor = new SkinEditorOverlay(ScalingContainer));
                skinEditor.SetTarget(soloResults);
            });
            AddStep("set argon skin", () => skins.CurrentSkinInfo.SetDefault());
            AddStep("set legacy skin", () => skins.CurrentSkinInfo.Value = skins.DefaultClassicSkin.SkinInfo);
            AddStep("set retro skin", () => skins.CurrentSkinInfo.Value = skins.RetroSkin.SkinInfo);
            AddStep("start animating", () => soloResults.StartAnimating());
            AddStep("finish animating", () => soloResults.FinishAnimating());
            AddToggleStep("toggle skin editor", b => skinEditor.State.Value = b ? Visibility.Visible : Visibility.Hidden);
        }

        private IScoreInfo createTestScore()
        {
            var rulesetInfo = rulesets.GetRuleset(RNG.Next(0, 4));
            var rulesetInstance = rulesetInfo!.CreateInstance();
            var score = TestResources.CreateTestScoreInfo(rulesetInfo);

            score.OnlineID = onlineScoreID++;
            score.HitEvents = TestSceneStatisticsPanel.CreatePositionDistributedHitEvents();
            score.Accuracy = RNG.NextDouble(0.7, 1);
            score.Rank = rulesetInstance.CreateScoreProcessor().RankFromScore(score.Accuracy, score.Statistics);
            score.PP = 138.4234;

            score.Statistics[HitResult.Miss] = RNG.NextBool() ? 2 : 0;
            score.TotalScore = RNG.Next(0, 1_200_001);

            score.BeatmapInfo = Beatmap.Value.BeatmapInfo;
            score.MaxCombo = RNG.NextBool() ? score.GetMaximumAchievableCombo() : 999;

            for (int i = 0; i < 500; ++i)
                score.HealthGraph.Add(new HealthValue(1500 * i, RNG.NextDouble()));

            return score;
        }
    }
}
