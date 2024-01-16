// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
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
    public partial class TestSceneStartTimeOrderedHitPolicy : RateAdjustedBeatmapTestScene
    {
        private const double early_miss_window = 1000; // time after -1000 to -500 is considered a miss
        private const double late_miss_window = 500; // time after +500 is considered a miss

        /// <summary>
        /// Tests clicking a future circle before the first circle's start time, while the first circle HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleBeforeFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;
            Vector2 positionFirstCircle = Vector2.Zero;
            Vector2 positionSecondCircle = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 100, Position = positionSecondCircle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Miss);
            addJudgementOffsetAssert(hitObjects[0], late_miss_window);
        }

        /// <summary>
        /// Tests clicking a future circle at the first circle's start time, while the first circle HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleAtFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;
            Vector2 positionFirstCircle = Vector2.Zero;
            Vector2 positionSecondCircle = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle, Position = positionSecondCircle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 0);
        }

        /// <summary>
        /// Tests clicking a future circle after the first circle's start time, while the first circle HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleAfterFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;
            Vector2 positionFirstCircle = Vector2.Zero;
            Vector2 positionSecondCircle = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle + 100, Position = positionSecondCircle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 100);
        }

        /// <summary>
        /// Tests clicking a future circle before the first circle's start time, while the first circle HAS been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleBeforeFirstCircleTimeWithFirstCircleJudged()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;
            Vector2 positionFirstCircle = Vector2.Zero;
            Vector2 positionSecondCircle = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 200, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle - 100, Position = positionSecondCircle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], -200); // time_first_circle - 200
            addJudgementOffsetAssert(hitObjects[1], -200); // time_second_circle - first_circle_time - 100
        }

        /// <summary>
        /// Tests clicking a future circle after a slider's start time, but hitting all slider ticks.
        /// </summary>
        [Test]
        public void TestMissSliderHeadAndHitAllSliderTicks()
        {
            const double time_slider = 1500;
            const double time_circle = 1510;
            Vector2 positionCircle = Vector2.Zero;
            Vector2 positionSlider = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new TestSlider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_slider, Position = positionCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_slider + 10, Position = positionSlider, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.IgnoreHit);
            addJudgementAssert("slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.Miss);
            addJudgementAssert("slider tick", () => ((Slider)hitObjects[1]).NestedHitObjects[1] as SliderTick, HitResult.LargeTickHit);
        }

        /// <summary>
        /// Tests clicking hitting future slider ticks before a circle.
        /// </summary>
        [Test]
        public void TestHitSliderTicksBeforeCircle()
        {
            const double time_slider = 1500;
            const double time_circle = 1510;
            Vector2 positionCircle = Vector2.Zero;
            Vector2 positionSlider = new Vector2(30);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new TestSlider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_slider, Position = positionSlider, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_circle + late_miss_window - 100, Position = positionCircle, Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_circle + late_miss_window - 90, Position = positionSlider, Actions = { OsuAction.LeftButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.IgnoreHit);
            addJudgementAssert("slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.Great);
            addJudgementAssert("slider tick", () => ((Slider)hitObjects[1]).NestedHitObjects[1] as SliderTick, HitResult.LargeTickHit);
        }

        /// <summary>
        /// Tests clicking a future circle before a spinner.
        /// </summary>
        [Test]
        public void TestHitCircleBeforeSpinner()
        {
            const double time_spinner = 1500;
            const double time_circle = 1800;
            Vector2 positionCircle = Vector2.Zero;

            var hitObjects = new List<OsuHitObject>
            {
                new TestSpinner
                {
                    StartTime = time_spinner,
                    Position = new Vector2(256, 192),
                    EndTime = time_spinner + 1000,
                },
                new TestHitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_spinner - 100, Position = positionCircle, Actions = { OsuAction.LeftButton } },
            };

            frames.AddRange(new SpinFramesGenerator(time_spinner + 10)
                            .Spin(360, 500)
                            .Build());

            performTest(hitObjects, frames);

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Great);
        }

        [Test]
        public void TestHitSliderHeadBeforeHitCircle()
        {
            const double time_circle = 1000;
            const double time_slider = 1200;
            Vector2 positionCircle = Vector2.Zero;
            Vector2 positionSlider = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new TestSlider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_circle - 100, Position = positionSlider, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_circle, Position = positionCircle, Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_slider, Position = positionSlider, Actions = { OsuAction.LeftButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.IgnoreHit);
        }

        [Test]
        public void TestInputFallsThroughJudgedSliders()
        {
            const double time_first_slider = 1000;
            const double time_second_slider = 1250;
            Vector2 positionFirstSlider = new Vector2(100, 50);
            Vector2 positionSecondSlider = new Vector2(100, 80);
            var midpoint = (positionFirstSlider + positionSecondSlider) / 2;

            var hitObjects = new List<OsuHitObject>
            {
                new TestSlider
                {
                    StartTime = time_first_slider,
                    Position = positionFirstSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                },
                new TestSlider
                {
                    StartTime = time_second_slider,
                    Position = positionSecondSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_slider, Position = midpoint, Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_first_slider + 25, Position = midpoint, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_slider + 50, Position = midpoint },
            });

            addJudgementAssert("first slider head", () => ((Slider)hitObjects[0]).HeadCircle, HitResult.Great);
            addJudgementOffsetAssert("first slider head", () => ((Slider)hitObjects[0]).HeadCircle, 0);
            addJudgementAssert("second slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.Great);
            addJudgementOffsetAssert("second slider head", () => ((Slider)hitObjects[1]).HeadCircle, -200);
        }

        private void addJudgementAssert(OsuHitObject hitObject, HitResult result)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject).Type == result);
        }

        private void addJudgementAssert(string name, Func<OsuHitObject> hitObject, HitResult result)
        {
            AddAssert($"{name} judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject()).Type == result);
        }

        private void addJudgementOffsetAssert(OsuHitObject hitObject, double offset)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judged at {offset}",
                () => Precision.AlmostEquals(judgementResults.Single(r => r.HitObject == hitObject).TimeOffset, offset, 100));
        }

        private void addJudgementOffsetAssert(string name, Func<OsuHitObject> hitObject, double offset)
        {
            AddAssert($"{name} @ judged at {offset}",
                () => judgementResults.Single(r => r.HitObject == hitObject()).TimeOffset, () => Is.EqualTo(offset).Within(50));
        }

        private ScoreAccessibleReplayPlayer currentPlayer;
        private List<Judgement> judgementResults;

        private void performTest(List<OsuHitObject> hitObjects, List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects = hitObjects,
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty { SliderTickRate = 3 },
                        Ruleset = new OsuRuleset().RulesetInfo
                    },
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
                judgementResults = new List<Judgement>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
        }

        private class TestHitCircle : HitCircle
        {
            protected override HitWindows CreateHitWindows() => new TestHitWindows();
        }

        private class TestSlider : Slider
        {
            public TestSlider()
            {
                SliderVelocityMultiplier = 0.1f;

                DefaultsApplied += _ =>
                {
                    HeadCircle.HitWindows = new TestHitWindows();
                    TailCircle.HitWindows = new TestHitWindows();

                    HeadCircle.HitWindows.SetDifficulty(0);
                    TailCircle.HitWindows.SetDifficulty(0);
                };
            }
        }

        private class TestSpinner : Spinner
        {
            protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
            {
                base.ApplyDefaultsToSelf(controlPointInfo, difficulty);
                SpinsRequired = 1;
            }
        }

        private class TestHitWindows : HitWindows
        {
            private static readonly DifficultyRange[] ranges =
            {
                new DifficultyRange(HitResult.Great, 500, 500, 500),
                new DifficultyRange(HitResult.Miss, early_miss_window, early_miss_window, early_miss_window),
            };

            public override bool IsHitResultAllowed(HitResult result) => result == HitResult.Great || result == HitResult.Miss;

            protected override DifficultyRange[] GetRanges() => ranges;
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
