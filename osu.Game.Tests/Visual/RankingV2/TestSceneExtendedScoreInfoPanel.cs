// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Extended;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneExtendedScoreInfoPanel : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => setScore(new OsuRuleset().RulesetInfo));

            AddStep("create component", () => Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.9f,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new ScoreInfoPanel()
                ],
            });
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle classic scoring mode", b =>
            {
                scoringMode.Value = b ? ScoringMode.Classic : ScoringMode.Standardised;
            });
            AddStep("osu! ruleset", () => setScore(new OsuRuleset().RulesetInfo));
            AddStep("taiko ruleset", () => setScore(new TaikoRuleset().RulesetInfo, s =>
            {
                s.Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1403,
                    [HitResult.Ok] = 250,
                    [HitResult.Miss] = 17,
                    [HitResult.LargeBonus] = 33,
                    [HitResult.SmallBonus] = 15,
                };
                s.MaximumStatistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1670,
                    [HitResult.LargeBonus] = 45,
                    [HitResult.SmallBonus] = 18
                };
            }));
            AddStep("catch ruleset", () => setScore(new CatchRuleset().RulesetInfo, s =>
            {
                s.Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1489,
                    [HitResult.Miss] = 124,
                    [HitResult.LargeTickHit] = 180,
                    [HitResult.SmallTickHit] = 504,
                    [HitResult.LargeBonus] = 81,
                };
                s.MaximumStatistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1613,
                    [HitResult.LargeTickHit] = 202,
                    [HitResult.SmallTickHit] = 763,
                    [HitResult.LargeBonus] = 101,
                };
            }));
            AddStep("mania ruleset", () => setScore(new ManiaRuleset().RulesetInfo, s =>
            {
                s.Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Perfect] = 500,
                    [HitResult.Great] = 1000,
                    [HitResult.Good] = 200,
                    [HitResult.Ok] = 100,
                    [HitResult.Meh] = 50,
                    [HitResult.Miss] = 25
                };
                s.MaximumStatistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1875,
                };
            }));

            AddStep("no mods", () => setScore(new OsuRuleset().RulesetInfo, s => s.Mods = []));
            AddStep("mods with settings", () => setScore(new OsuRuleset().RulesetInfo, s => s.Mods = new Mod[]
            {
                new OsuModDifficultyAdjust { CircleSize = { Value = 3 } },
                new OsuModDoubleTime { SpeedChange = { Value = 1.3 } },
            }));
            AddStep("many mods", () => setScore(new OsuRuleset().RulesetInfo, s => s.Mods = new Mod[]
            {
                new OsuModNoFail(),
                new OsuModHardRock(),
                new OsuModHidden(),
                new OsuModFlashlight(),
                new OsuModStrictTracking(),
                new OsuModSpunOut(),
                new OsuModRandom(),
                new OsuModSingleTap(),
                new OsuModBubbles(),
                new OsuModDeflate(),
                new ModWindUp(),
                new OsuModMuted(),
                new OsuModNoScope(),
                new OsuModSynesthesia(),
                new OsuModTransform(),
            }));
        }

        private void setScore(RulesetInfo ruleset, Action<ScoreInfo>? setUp = null)
        {
            var newScore = new ScoreInfo
            {
                BeatmapInfo = CreateBeatmap(ruleset).BeatmapInfo,
                Ruleset = ruleset,
                Accuracy = 0.8475,
                MaxCombo = 727,
                PP = 1003.234,
                TotalScore = 1_123_456,
                Statistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1343,
                    [HitResult.Ok] = 443,
                    [HitResult.Meh] = 12,
                    [HitResult.Miss] = 3,
                    [HitResult.LargeTickHit] = 78,
                    [HitResult.LargeTickMiss] = 3,
                    [HitResult.SmallTickHit] = 44,
                    [HitResult.SmallTickMiss] = 10,
                    [HitResult.LargeBonus] = 10,
                    [HitResult.IgnoreMiss] = 3,
                },
                MaximumStatistics = new Dictionary<HitResult, int>
                {
                    [HitResult.Great] = 1801,
                    [HitResult.LargeTickHit] = 81,
                    [HitResult.SmallTickHit] = 54,
                    [HitResult.LargeBonus] = 13,
                },
                Mods = [new OsuModHidden(), new OsuModDoubleTime()]
            };

            setUp?.Invoke(newScore);

            score.Value = newScore;
        }
    }
}
