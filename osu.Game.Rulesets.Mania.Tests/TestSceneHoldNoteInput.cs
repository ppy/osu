// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneHoldNoteInput : RateAdjustedBeatmapTestScene
    {
        private const double time_before_head = 250;
        private const double time_head = 1500;
        private const double time_during_hold_1 = 2500;
        private const double time_tail = 4000;
        private const double time_after_tail = 5250;

        private List<JudgementResult> judgementResults;

        /// <summary>
        ///     -----[           ]-----
        ///  o                           o
        /// </summary>
        [Test]
        public void TestNoInput()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_before_head),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Miss);
            assertNoteJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        ///     -----[           ]-----
        ///  x                           o
        /// </summary>
        [Test]
        public void TestPressTooEarlyAndReleaseAfterTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_before_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_after_tail, ManiaAction.Key1),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///  x                   o
        /// </summary>
        [Test]
        public void TestPressTooEarlyAndReleaseAtTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_before_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///  xo      x                   o
        /// </summary>
        [Test]
        public void TestPressTooEarlyThenPressAtStartAndReleaseAfterTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_before_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_before_head + 10),
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///  xo      x           o
        /// </summary>
        [Test]
        public void TestPressTooEarlyThenPressAtStartAndReleaseAtTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_before_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_before_head + 10),
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Perfect);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          xo                  o
        /// </summary>
        [Test]
        public void TestPressAtStartAndBreak()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_head + 10),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          xo  x               o
        /// </summary>
        [Test]
        public void TestPressAtStartThenBreakThenRepressAndReleaseAfterTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_head + 10),
                new ManiaReplayFrame(time_during_hold_1, ManiaAction.Key1),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          xo  x       o       o
        /// </summary>
        [Test]
        public void TestPressAtStartThenBreakThenRepressAndReleaseAtTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_head + 10),
                new ManiaReplayFrame(time_during_hold_1, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Meh);
        }

        /// <summary>
        ///     -----[           ]-----
        ///             x                o
        /// </summary>
        [Test]
        public void TestPressDuringNoteAndReleaseAfterTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_during_hold_1, ManiaAction.Key1),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///             x        o       o
        /// </summary>
        [Test]
        public void TestPressDuringNoteAndReleaseAtTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_during_hold_1, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.Meh);
        }

        /// <summary>
        ///     -----[           ]-----
        ///                      xo      o
        /// </summary>
        [Test]
        public void TestPressAndReleaseAtTail()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_tail, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail + 10),
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Meh);
        }

        [Test]
        public void TestMissReleaseAndHitSecondRelease()
        {
            var windows = new ManiaHitWindows();
            windows.SetDifficulty(10);

            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = 1000,
                        Duration = 500,
                        Column = 0,
                    },
                    new HoldNote
                    {
                        StartTime = 1000 + 500 + windows.WindowFor(HitResult.Miss) + 10,
                        Duration = 500,
                        Column = 0,
                    },
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        SliderTickRate = 4,
                        OverallDifficulty = 10,
                    },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(beatmap.HitObjects[1].StartTime, ManiaAction.Key1),
                new ManiaReplayFrame(beatmap.HitObjects[1].GetEndTime()),
            }, beatmap);

            AddAssert("first hold note missed", () => judgementResults.Where(j => beatmap.HitObjects[0].NestedHitObjects.Contains(j.HitObject))
                                                                      .All(j => !j.Type.IsHit()));

            AddAssert("second hold note missed", () => judgementResults.Where(j => beatmap.HitObjects[1].NestedHitObjects.Contains(j.HitObject))
                                                                       .All(j => j.Type.IsHit()));
        }

        [Test]
        public void TestHitTailBeforeLastTick()
        {
            const int tick_rate = 8;
            const double tick_spacing = TimingControlPoint.DEFAULT_BEAT_LENGTH / tick_rate;
            const double time_last_tick = time_head + tick_spacing * (int)((time_tail - time_head) / tick_spacing - 1);

            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = time_head,
                        Duration = time_tail - time_head,
                        Column = 0,
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = tick_rate },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_last_tick - 5)
            }, beatmap);

            assertHeadJudgement(HitResult.Perfect);
            assertLastTickJudgement(HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.Ok);
        }

        [Test]
        public void TestZeroLength()
        {
            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = 1000,
                        Duration = 0,
                        Column = 0,
                    },
                },
                BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(beatmap.HitObjects[0].StartTime, ManiaAction.Key1),
                new ManiaReplayFrame(beatmap.HitObjects[0].GetEndTime() + 1),
            }, beatmap);

            AddAssert("hold note hit", () => judgementResults.Where(j => beatmap.HitObjects[0].NestedHitObjects.Contains(j.HitObject))
                                                             .All(j => j.Type.IsHit()));
        }

        private void assertHeadJudgement(HitResult result)
            => AddAssert($"head judged as {result}", () => judgementResults.First(j => j.HitObject is Note).Type == result);

        private void assertTailJudgement(HitResult result)
            => AddAssert($"tail judged as {result}", () => judgementResults.Single(j => j.HitObject is TailNote).Type == result);

        private void assertNoteJudgement(HitResult result)
            => AddAssert($"hold note judged as {result}", () => judgementResults.Single(j => j.HitObject is HoldNote).Type == result);

        private void assertTickJudgement(HitResult result)
            => AddAssert($"any tick judged as {result}", () => judgementResults.Where(j => j.HitObject is HoldNoteTick).Any(j => j.Type == result));

        private void assertLastTickJudgement(HitResult result)
            => AddAssert($"last tick judged as {result}", () => judgementResults.Last(j => j.HitObject is HoldNoteTick).Type == result);

        private ScoreAccessibleReplayPlayer currentPlayer;

        private void performTest(List<ReplayFrame> frames, Beatmap<ManiaHitObject> beatmap = null)
        {
            if (beatmap == null)
            {
                beatmap = new Beatmap<ManiaHitObject>
                {
                    HitObjects =
                    {
                        new HoldNote
                        {
                            StartTime = time_head,
                            Duration = time_tail - time_head,
                            Column = 0,
                        }
                    },
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                        Ruleset = new ManiaRuleset().RulesetInfo
                    },
                };

                beatmap.ControlPointInfo.Add(0, new EffectControlPoint { ScrollSpeed = 0.1f });
            }

            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                };

                LoadScreen(currentPlayer = p);
                judgementResults = new List<JudgementResult>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());

            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor?.HasCompleted.Value == true);
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public new GameplayClockContainer GameplayClockContainer => base.GameplayClockContainer;

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
