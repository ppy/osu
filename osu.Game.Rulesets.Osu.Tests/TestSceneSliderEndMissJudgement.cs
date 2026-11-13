// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSliderEndMissJudgement : RateAdjustedBeatmapTestScene
    {
        private const double time_slider_start = 1000;
        private const float slider_path_length = 200;

        private static readonly Vector2 slider_start_position = new Vector2(256 - slider_path_length / 2, 192);

        private OsuRulesetConfigManager config = null!;

        private ScoreAccessibleReplayPlayer currentPlayer = null!;
        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        [BackgroundDependencyLoader]
        private void load()
        {
            config = (OsuRulesetConfigManager)RulesetConfigs.GetConfigFor(new OsuRuleset()).AsNonNull();
        }

        [Test]
        public void TestMissMarkersHidden()
        {
            AddStep("disable slider tick miss markers", () => config.SetValue(OsuRulesetSetting.ShowSliderTickMissMarkers, false));

            performMiss();

            AddUntilStep("wait for tick miss", () => judgementResults.Any(r => r.Type == HitResult.LargeTickMiss));
            AddAssert("no slider miss markers shown", () => !currentPlayer.ChildrenOfType<DrawableOsuJudgement>()
                                                                         .Any(j => j.Result?.Type is HitResult.LargeTickMiss or HitResult.IgnoreMiss));
        }

        [Test]
        public void TestMissMarkersShown()
        {
            AddStep("enable slider tick miss markers", () => config.SetValue(OsuRulesetSetting.ShowSliderTickMissMarkers, true));

            performMiss();

            AddUntilStep("slider miss markers shown", () => currentPlayer.ChildrenOfType<DrawableOsuJudgement>()
                                                                        .Any(j => j.Result?.Type is HitResult.LargeTickMiss or HitResult.IgnoreMiss));
        }

        private void performMiss()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 50, slider_start_position),
            });
        }

        private void performTest(List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects =
                    {
                        new Slider
                        {
                            StartTime = time_slider_start,
                            Position = slider_start_position,
                            TickDistanceMultiplier = 3,
                            Path = new SliderPath(PathType.LINEAR, new[]
                            {
                                Vector2.Zero,
                                new Vector2(slider_path_length, 0),
                            }, slider_path_length),
                        }
                    },
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty
                        {
                            SliderMultiplier = 1,
                            SliderTickRate = 3,
                            OverallDifficulty = 0
                        },
                        Ruleset = new OsuRuleset().RulesetInfo,
                    }
                });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults.Clear();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
        }

        private partial class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, new PlayerConfiguration
                {
                    AllowPause = false,
                    ShowResults = false,
                })
            {
            }
        }
    }
}
