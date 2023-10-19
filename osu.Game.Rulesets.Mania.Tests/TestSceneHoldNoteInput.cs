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
    /// <summary>
    /// Diagrams in this class are represented as:
    /// -   : time
    /// O   : note
    /// [ ] : hold note
    ///
    /// x   : button press
    /// o   : button release
    /// </summary>
    public partial class TestSceneHoldNoteInput : RateAdjustedBeatmapTestScene
    {
        private const double time_before_head = 250;
        private const double time_head = 1500;
        private const double time_during_hold_1 = 2500;
        private const double time_tail = 4000;
        private const double time_after_tail = 5250;

        private List<JudgementResult> judgementResults = new List<JudgementResult>();

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
            assertTailJudgement(HitResult.Miss);
            assertNoteJudgement(HitResult.IgnoreMiss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          x           o
        /// </summary>
        [Test]
        public void TestCorrectInput()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTailJudgement(HitResult.Perfect);
            assertNoteJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          x                   o
        /// </summary>
        [Test]
        public void TestLateRelease()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_after_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            assertTailJudgement(HitResult.Miss);
            assertNoteJudgement(HitResult.IgnoreMiss);
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
            assertTailJudgement(HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]-----
        ///          xox         o
        /// </summary>
        [Test]
        public void TestPressAtStartThenReleaseAndImmediatelyRepress()
        {
            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head, ManiaAction.Key1),
                new ManiaReplayFrame(time_head + 1),
                new ManiaReplayFrame(time_head + 2, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            });

            assertHeadJudgement(HitResult.Perfect);
            // judgement combo offset by perfect bonus judgement. see logic in DrawableNote.CheckForResult.
            assertComboAtJudgement(1, 1);
            assertTailJudgement(HitResult.Meh);
            assertComboAtJudgement(2, 0);
            // judgement combo offset by perfect bonus judgement. see logic in DrawableNote.CheckForResult.
            assertComboAtJudgement(4, 1);
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
            assertTailJudgement(HitResult.Meh);
        }

        /// <summary>
        ///     -----[ ]-O-------------
        ///           xo                 o
        /// </summary>
        [Test]
        public void TestPressAndReleaseJustBeforeTailWithNearbyNoteAndCloseByHead()
        {
            Note note;

            const int duration = 50;

            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    // hold note is very short, to make the head still in range
                    new HoldNote
                    {
                        StartTime = time_head,
                        Duration = duration,
                        Column = 0,
                    },
                    {
                        // Next note within tail lenience
                        note = new Note
                        {
                            StartTime = time_head + duration + 10
                        }
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_head + duration, ManiaAction.Key1),
                new ManiaReplayFrame(time_head + duration + 10),
            }, beatmap);

            assertHeadJudgement(HitResult.Good);
            assertTailJudgement(HitResult.Perfect);

            assertHitObjectJudgement(note, HitResult.Miss);
        }

        /// <summary>
        ///     -----[           ]--O--
        ///                     xo       o
        /// </summary>
        [Test]
        public void TestPressAndReleaseJustBeforeTailWithNearbyNote()
        {
            Note note;

            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = time_head,
                        Duration = time_tail - time_head,
                        Column = 0,
                    },
                    {
                        // Next note within tail lenience
                        note = new Note
                        {
                            StartTime = time_tail + 50
                        }
                    }
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_tail - 10, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail),
            }, beatmap);

            assertHeadJudgement(HitResult.Miss);
            assertTailJudgement(HitResult.Miss);

            assertHitObjectJudgement(note, HitResult.Good);
        }

        /// <summary>
        ///     -----[           ]--O--
        ///                       xo     o
        /// </summary>
        [Test]
        public void TestPressAndReleaseJustAfterTailWithNearbyNote()
        {
            // Next note within tail lenience
            Note note = new Note { StartTime = time_tail + 50 };

            var beatmap = new Beatmap<ManiaHitObject>
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = time_head,
                        Duration = time_tail - time_head,
                        Column = 0,
                    },
                    note
                },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty { SliderTickRate = 4 },
                    Ruleset = new ManiaRuleset().RulesetInfo
                },
            };

            performTest(new List<ReplayFrame>
            {
                new ManiaReplayFrame(time_tail + 10, ManiaAction.Key1),
                new ManiaReplayFrame(time_tail + 20),
            }, beatmap);

            assertHeadJudgement(HitResult.Miss);
            assertTailJudgement(HitResult.Miss);

            assertHitObjectJudgement(note, HitResult.Great);
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

        private void assertHitObjectJudgement(HitObject hitObject, HitResult result)
            => AddAssert($"object judged as {result}", () => judgementResults.First(j => j.HitObject == hitObject).Type, () => Is.EqualTo(result));

        private void assertHeadJudgement(HitResult result)
            => AddAssert($"head judged as {result}", () => judgementResults.First(j => j.HitObject is Note).Type, () => Is.EqualTo(result));

        private void assertTailJudgement(HitResult result)
            => AddAssert($"tail judged as {result}", () => judgementResults.Single(j => j.HitObject is TailNote).Type, () => Is.EqualTo(result));

        private void assertNoteJudgement(HitResult result)
            => AddAssert($"hold note judged as {result}", () => judgementResults.Single(j => j.HitObject is HoldNote).Type, () => Is.EqualTo(result));

        private void assertComboAtJudgement(int judgementIndex, int combo)
            => AddAssert($"combo at judgement {judgementIndex} is {combo}", () => judgementResults.ElementAt(judgementIndex).ComboAfterJudgement, () => Is.EqualTo(combo));

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private void performTest(List<ReplayFrame> frames, Beatmap<ManiaHitObject>? beatmap = null)
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
