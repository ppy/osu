// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSliderEarlyHitJudgement : RateAdjustedBeatmapTestScene
    {
        private const double time_slider_start = 1000;
        private const double time_slider_end = 3000;

        private static readonly Vector2 slider_start_position = new Vector2(256 - slider_path_length / 2, 192);
        private static readonly Vector2 slider_end_position = new Vector2(256 + slider_path_length / 2, 192);
        private static readonly Vector2 offset_inside_follow = new Vector2(35, 0);
        private static readonly Vector2 offset_outside_follow = offset_inside_follow * 2;

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private const float slider_path_length = 200;

        private readonly List<Judgement> judgementResults = new List<Judgement>();

        [Test]
        public void TestHitEarlyMoveIntoFollowRegion()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 100, slider_start_position + offset_inside_follow, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end - 100, slider_end_position + offset_inside_follow, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitEarlyAndReleaseInFollowRegion()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 100, slider_start_position + offset_inside_follow, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 50, slider_start_position + offset_inside_follow),
                new OsuReplayFrame(time_slider_end - 50, slider_end_position + offset_inside_follow, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitEarlyAndRepressInFollowRegion()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 100, slider_start_position + offset_inside_follow, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 75, slider_start_position + offset_inside_follow),
                new OsuReplayFrame(time_slider_start - 50, slider_start_position + offset_inside_follow, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end - 50, slider_end_position + offset_inside_follow, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitEarlyMoveOutsideFollowRegion()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start - 100, slider_start_position + offset_outside_follow, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end - 100, slider_end_position + offset_outside_follow, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        private void assertHeadJudgement(HitResult result)
        {
            AddAssert(
                "check head result",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderHeadCircle)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertTickJudgement(HitResult result)
        {
            AddAssert(
                "check tick result",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderTick)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertRepeatJudgement(HitResult result)
        {
            AddAssert(
                "check tick result",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderRepeat)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertTailJudgement(HitResult result)
        {
            AddAssert(
                "check tail result",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderTailCircle)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertSliderJudgement(HitResult result)
        {
            AddAssert(
                "check slider result",
                () => judgementResults.SingleOrDefault(r => r.HitObject is Slider)?.Type,
                () => Is.EqualTo(result));
        }

        private Vector2 computePositionFromTime(double time)
        {
            Vector2 dist = slider_end_position - slider_start_position;
            double t = (time - time_slider_start) / (time_slider_end - time_slider_start);
            return slider_start_position + dist * (float)t;
        }

        private void performTest(List<ReplayFrame> frames, Action<Slider>? adjustSliderFunc = null, bool classic = false)
        {
            Slider slider = new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(256 - slider_path_length / 2, 192),
                TickDistanceMultiplier = 3,
                ClassicSliderBehaviour = classic,
                Samples = new[]
                {
                    new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                },
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(slider_path_length, 0),
                }, slider_path_length),
            };

            adjustSliderFunc?.Invoke(slider);

            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects = { slider },
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
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
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
