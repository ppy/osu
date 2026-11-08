// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Legacy;
using osuTK;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneLegacyRankingPanel : SkinnableTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();
        private long standardisedTotalScore = 869_310;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => setScore(new OsuRuleset().RulesetInfo));

            AddStep("create component", () => SetContents(_ => new LegacyRankingPanel
            {
                Scale = new Vector2(0.5f),
            }));
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle outline", t =>
            {
                foreach (var outline in this.ChildrenOfType<OutlineBox>())
                    outline.Alpha = t ? 1 : 0;
            });
            AddStep("begin animation", () =>
            {
                foreach (var panel in this.ChildrenOfType<LegacyRankingPanel>())
                    panel.StartAnimating(Time.Current);
            });
            AddStep("cancel animation", () =>
            {
                foreach (var panel in this.ChildrenOfType<LegacyRankingPanel>())
                    panel.FinishAnimating();
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
            AddStep("custom ruleset", () => setScore(new TestRuleset().RulesetInfo, s =>
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
            AddSliderStep("change total score", 0L, 1_200_000L, 750_000L, s =>
            {
                standardisedTotalScore = s;
                setScore(new OsuRuleset().RulesetInfo);
            });
            AddToggleStep("toggle classic scoring mode", b =>
            {
                scoringMode.Value = b ? ScoringMode.Classic : ScoringMode.Standardised;
                setScore(new OsuRuleset().RulesetInfo);
            });
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
                Mods = [new OsuModHidden(), new OsuModDoubleTime()],
                TotalScore = standardisedTotalScore,
            };

            setUp?.Invoke(newScore);

            score.Value = newScore;
        }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        private class TestRuleset : Ruleset
        {
            public override IEnumerable<HitResult> GetValidHitResults() => new[]
            {
                HitResult.Perfect,
                HitResult.Great,
                HitResult.Good,
                HitResult.Ok,
                HitResult.Meh,
                HitResult.Miss,
            };

            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => throw new NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description => string.Empty;
            public override string ShortName => string.Empty;
        }
    }
}
