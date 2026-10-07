// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Argon;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneArgonTotalScorePanel : OsuTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private ArgonTotalScorePanel panel = null!;
        private SkinnableTestScene.OutlineBox? outline;

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();
        private long standardisedTotalScore;
        private int maxCombo = 500;
        private bool perfect;
        private bool isPersonalBest;

        private BeatmapInfo beatmap = null!;
        private const int local_personal_best_score_id = 6000;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set up local best", () =>
            {
                Realm.Write(r =>
                {
                    var ruleset = r.Find<RulesetInfo>(OsuRuleset.SHORT_NAME)!;
                    var b = CreateBeatmap(ruleset).BeatmapInfo;
                    r.Add(new ScoreInfo
                    {
                        User = API.LocalUser.Value,
                        BeatmapInfo = b,
                        BeatmapHash = b.Hash,
                        OnlineID = local_personal_best_score_id,
                        Ruleset = ruleset,
                    });
                    beatmap = b.Detach();
                });
            });
            AddStep("set score", () =>
            {
                standardisedTotalScore = RNG.Next(0, 1_200_001);
                refreshScore();
            });

            AddStep("create component", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    panel = new ArgonTotalScorePanel(),
                    outline = new SkinnableTestScene.OutlineBox(),
                ],
            });
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle outline", t => outline?.Alpha = t ? 1 : 0);
            AddStep("begin animation", () => panel.StartAnimating(Time.Current));
            AddStep("cancel animation", () => panel.FinishAnimating());

            AddSliderStep("change total score", 0L, 1_200_000L, 750_000L, s =>
            {
                standardisedTotalScore = s;
                if (panel.IsNotNull())
                    refreshScore();
            });
            AddSliderStep("change max combo", 0, 10_000, 500, m =>
            {
                maxCombo = m;
                if (panel.IsNotNull())
                    refreshScore();
            });
            AddToggleStep("toggle classic scoring mode", b =>
            {
                scoringMode.Value = b ? ScoringMode.Classic : ScoringMode.Standardised;
                refreshScore();
            });
            AddToggleStep("toggle perfect", b =>
            {
                perfect = b;
                refreshScore();
            });
            AddToggleStep("toggle personal best", b =>
            {
                isPersonalBest = b;
                refreshScore();
            });
        }

        private void refreshScore()
        {
            int greats = (int)(maxCombo * (perfect ? 1 : 0.93f));

            score.Value = new ScoreInfo
            {
                User = API.LocalUser.Value,
                TotalScore = standardisedTotalScore,
                Ruleset = new OsuRuleset().RulesetInfo,
                MaxCombo = (int)(maxCombo * (perfect ? 1 : 0.7)),
                Statistics =
                {
                    [HitResult.Great] = greats,
                    [HitResult.Miss] = maxCombo - greats,
                },
                MaximumStatistics =
                {
                    [HitResult.Great] = maxCombo,
                },
                BeatmapInfo = beatmap,
                BeatmapHash = beatmap.Hash,
                OnlineID = isPersonalBest ? local_personal_best_score_id : -1,
            };
        }
    }
}
