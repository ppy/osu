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
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Screens.RankingV2.Argon;

namespace osu.Game.Tests.Visual.RankingV2
{
    public partial class TestSceneArgonScoreRankDisplay : OsuTestScene
    {
        [Cached(typeof(IBindable<IScoreInfo>))]
        private readonly Bindable<IScoreInfo> score = new Bindable<IScoreInfo>();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private ArgonScoreRankDisplay display = null!;
        private SkinnableTestScene.OutlineBox? outline;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set score", () => setScore(new OsuRuleset().RulesetInfo, s =>
            {
            }));

            AddStep("create component", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    display = new ArgonScoreRankDisplay(),
                    outline = new SkinnableTestScene.OutlineBox(),
                ],
            });
        }

        [Test]
        public void TestAppearance()
        {
            AddToggleStep("toggle outline", t => outline?.Alpha = t ? 1 : 0);
            AddStep("begin animation", () => display.StartAnimating(Time.Current));
            AddStep("cancel animation", () => display.FinishAnimating());

            AddStep("osu! ruleset", () => setScore(new OsuRuleset().RulesetInfo));
            AddStep("taiko ruleset", () => setScore(new TaikoRuleset().RulesetInfo));
            AddStep("catch ruleset", () => setScore(new CatchRuleset().RulesetInfo));
            AddStep("mania ruleset", () => setScore(new ManiaRuleset().RulesetInfo));

            foreach (var scoreRank in Enum.GetValues<ScoreRank>())
            {
                AddStep($"rank {scoreRank}", () =>
                {
                    var processor = new OsuScoreProcessor();
                    double accuracy;

                    switch (scoreRank)
                    {
                        case ScoreRank.F:
                            accuracy = RNG.NextDouble();
                            break;

                        case ScoreRank.D:
                            accuracy = RNG.NextDouble(0, processor.AccuracyCutoffFromRank(ScoreRank.C));
                            break;

                        case ScoreRank.C:
                            accuracy = RNG.NextDouble(processor.AccuracyCutoffFromRank(ScoreRank.C), processor.AccuracyCutoffFromRank(ScoreRank.B));
                            break;

                        case ScoreRank.B:
                            accuracy = RNG.NextDouble(processor.AccuracyCutoffFromRank(ScoreRank.B), processor.AccuracyCutoffFromRank(ScoreRank.A));
                            break;

                        case ScoreRank.A:
                            accuracy = RNG.NextDouble(processor.AccuracyCutoffFromRank(ScoreRank.A), processor.AccuracyCutoffFromRank(ScoreRank.S));
                            break;

                        case ScoreRank.S:
                        case ScoreRank.SH:
                            accuracy = RNG.NextDouble(processor.AccuracyCutoffFromRank(ScoreRank.S), processor.AccuracyCutoffFromRank(ScoreRank.X));
                            break;

                        case ScoreRank.X:
                        case ScoreRank.XH:
                            accuracy = 1;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    setScore(new OsuRuleset().RulesetInfo, s =>
                    {
                        s.Accuracy = accuracy;
                        s.Rank = scoreRank;
                    });
                });
            }

            AddStep("rank S -> A due to misses", () =>
            {
                setScore(new OsuRuleset().RulesetInfo, s =>
                {
                    s.Accuracy = 0.9995;
                    s.Statistics = new Dictionary<HitResult, int>
                    {
                        [HitResult.Great] = 1999,
                        [HitResult.Miss] = 1,
                    };
                    s.Rank = ScoreRank.A;
                });
            });
        }

        private void setScore(RulesetInfo ruleset, Action<ScoreInfo>? setUp = null)
        {
            var newScore = new ScoreInfo
            {
                BeatmapInfo = CreateBeatmap(ruleset).BeatmapInfo,
                Ruleset = ruleset,
                Accuracy = 0.9475,
            };
            newScore.Rank = ruleset.CreateInstance().CreateScoreProcessor().RankFromScore(newScore.Accuracy, new Dictionary<HitResult, int>());

            setUp?.Invoke(newScore);

            score.Value = newScore;
        }
    }
}
