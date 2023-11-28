// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#pragma warning disable CS0618 // Type or member is obsolete

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
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
    public partial class TestSceneSliderJudgement : RateAdjustedBeatmapTestScene
    {
        private const double time_slider_start = 1000;
        private const double time_slider_tick = 2000;
        private const double time_slider_end = 3000;

        private static readonly Vector2 slider_start_position = new Vector2(256 - slider_path_length / 2, 192);
        private static readonly Vector2 slider_end_position = new Vector2(256 + slider_path_length / 2, 192);

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private const float slider_path_length = 200;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        #region Nomod

        [Test]
        public void TestHitAll()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyGreatNoCombo);
        }

        [Test]
        public void TestImperfectlyHitHead()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 100, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissHead()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 500, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyMehNoCombo);
        }

        [Test]
        public void TestMissTick()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_tick - 200)),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissTail()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200)),
                new OsuReplayFrame(time_slider_end, slider_end_position),
            });

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissHeadAndTick()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 500, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_tick - 200)),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position),
            });

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyMehNoCombo);
        }

        [Test]
        public void TestMissAll()
        {
            performTest(new List<ReplayFrame>());

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.Miss);
        }

        [Test]
        public void TestMissRepeat()
        {
            // This adjusts the slider so that the repeat is at time_slider_tick.
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_end - 400)),
                new OsuReplayFrame(time_slider_tick, slider_end_position),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_end - 400), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.RepeatCount = 1;
                s.SliderVelocityMultiplier = 2;
                s.TickDistanceMultiplier = 10;
            });

            assertHeadJudgement(HitResult.LargeTickHit);
            assertRepeatJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        #endregion

        #region Classic

        [Test]
        public void TestHitAllClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyGreatNoCombo);
        }

        [Test]
        public void TestImperfectlyHitHeadClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 100, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyGreatNoCombo);
        }

        [Test]
        public void TestMissHeadClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 500, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissTickClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_tick - 200)),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position, OsuAction.LeftButton),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissTailClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200)),
                new OsuReplayFrame(time_slider_end, slider_end_position),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickHit);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        [Test]
        public void TestMissHeadAndTickClassic()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start - 500, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_tick - 200)),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_tick + 200), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_end_position),
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyMehNoCombo);
        }

        [Test]
        public void TestMissAllClassic()
        {
            performTest(new List<ReplayFrame>(), classic: true);

            assertHeadJudgement(HitResult.LargeTickMiss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.Miss);
        }

        [Test]
        public void TestMissRepeatClassic()
        {
            // This adjusts the slider so that the repeat is at time_slider_tick.
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_tick - 200, computePositionFromTime(time_slider_end - 400)),
                new OsuReplayFrame(time_slider_tick, slider_end_position),
                new OsuReplayFrame(time_slider_tick + 200, computePositionFromTime(time_slider_end - 400), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.RepeatCount = 1;
                s.SliderVelocityMultiplier = 2;
                s.TickDistanceMultiplier = 10;
            }, classic: true);

            assertHeadJudgement(HitResult.LargeTickHit);
            assertRepeatJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.LargeTickHit);
            assertSliderJudgement(HitResult.LegacyOkNoCombo);
        }

        #endregion

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
                        Difficulty = new BeatmapDifficulty { SliderTickRate = 3 },
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
