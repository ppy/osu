// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneLegacyHitPolicy : RateAdjustedBeatmapTestScene
    {
        private readonly OsuHitWindows referenceHitWindows;

        /// <summary>
        /// This is provided as a convenience for testing note lock behaviour against osu!stable.
        /// Setting this field to a non-null path will cause beatmap files and replays used in all test cases
        /// to be exported to disk so that they can be cross-checked against stable.
        /// </summary>
        private readonly string? exportLocation = null;

        public TestSceneLegacyHitPolicy()
        {
            referenceHitWindows = new OsuHitWindows();
            referenceHitWindows.SetDifficulty(0);
        }

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
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new HitCircle
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
            // note lock prevented the object from being hit, so the judgement offset should be very late.
            addJudgementOffsetAssert(hitObjects[0], referenceHitWindows.WindowFor(HitResult.Meh));
            addClickActionAssert(0, ClickAction.Shake);
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
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new HitCircle
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
            addJudgementAssert(hitObjects[1], HitResult.Miss);
            // note lock prevented the object from being hit, so the judgement offset should be very late.
            addJudgementOffsetAssert(hitObjects[0], referenceHitWindows.WindowFor(HitResult.Meh));
            addClickActionAssert(0, ClickAction.Shake);
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
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new HitCircle
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
            addJudgementAssert(hitObjects[1], HitResult.Miss);
            // note lock prevented the object from being hit, so the judgement offset should be very late.
            addJudgementOffsetAssert(hitObjects[0], referenceHitWindows.WindowFor(HitResult.Meh));
            addClickActionAssert(0, ClickAction.Shake);
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
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new HitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 190, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle - 90, Position = positionSecondCircle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Meh);
            addJudgementAssert(hitObjects[1], HitResult.Meh);
            addJudgementOffsetAssert(hitObjects[0], -190); // time_first_circle - 190
            addJudgementOffsetAssert(hitObjects[1], -190); // time_second_circle - first_circle_time - 90
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
        }

        /// <summary>
        /// Tests clicking a future circle after the first circle's start time, while the first circle HAS been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleAfterFirstCircleTimeWithFirstCircleJudged()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;
            Vector2 positionFirstCircle = Vector2.Zero;
            Vector2 positionSecondCircle = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle
                },
                new HitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 190, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle, Position = positionSecondCircle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Meh);
            addJudgementAssert(hitObjects[1], HitResult.Ok);
            addJudgementOffsetAssert(hitObjects[0], -190); // time_first_circle - 190
            addJudgementOffsetAssert(hitObjects[1], -100); // time_second_circle - first_circle_time
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
        }

        /// <summary>
        /// Tests clicking a future circle after a slider's start time, but hitting the slider head and all slider ticks.
        /// </summary>
        [Test]
        public void TestHitCircleBeforeSliderHead()
        {
            const double time_slider = 1500;
            const double time_circle = 1510;
            Vector2 positionCircle = Vector2.Zero;
            Vector2 positionSlider = new Vector2(80);

            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new Slider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(50, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_slider, Position = positionCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_slider + 10, Position = positionSlider, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementAssert("slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.LargeTickHit);
            addJudgementAssert("slider tick", () => ((Slider)hitObjects[1]).NestedHitObjects[1] as SliderTick, HitResult.LargeTickHit);
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
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
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new Slider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(50, 0),
                    })
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_slider, Position = positionSlider, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_circle + referenceHitWindows.WindowFor(HitResult.Meh) - 100, Position = positionCircle, Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_circle + referenceHitWindows.WindowFor(HitResult.Meh) - 90, Position = positionSlider, Actions = { OsuAction.LeftButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Ok);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementAssert("slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.LargeTickHit);
            addJudgementAssert("slider tick", () => ((Slider)hitObjects[1]).NestedHitObjects[1] as SliderTick, HitResult.LargeTickHit);
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
        }

        /// <summary>
        /// Tests clicking a future circle before a spinner.
        /// </summary>
        [Test]
        public void TestHitCircleBeforeSpinner()
        {
            const double time_spinner = 1500;
            const double time_circle = 1600;
            Vector2 positionCircle = Vector2.Zero;

            var hitObjects = new List<OsuHitObject>
            {
                new TestSpinner
                {
                    StartTime = time_spinner,
                    Position = new Vector2(256, 192),
                    EndTime = time_spinner + 1000,
                },
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_spinner - 90, Position = positionCircle, Actions = { OsuAction.LeftButton } },
            };

            frames.AddRange(new SpinFramesGenerator(time_spinner + 10)
                            .Spin(360, 500)
                            .Build());

            performTest(hitObjects, frames);

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Meh);
            addClickActionAssert(0, ClickAction.Hit);
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
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new Slider
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
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addClickActionAssert(0, ClickAction.Shake);
            addClickActionAssert(1, ClickAction.Hit);
            addClickActionAssert(2, ClickAction.Hit);
        }

        [Test]
        public void TestOverlappingSliders()
        {
            const double time_first_slider = 1000;
            const double time_second_slider = 1200;
            Vector2 positionFirstSlider = new Vector2(100, 50);
            Vector2 positionSecondSlider = new Vector2(100, 80);
            var midpoint = (positionFirstSlider + positionSecondSlider) / 2;

            var hitObjects = new List<OsuHitObject>
            {
                new Slider
                {
                    StartTime = time_first_slider,
                    Position = positionFirstSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                },
                new Slider
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
                new OsuReplayFrame { Time = time_first_slider + 25, Position = midpoint, Actions = { OsuAction.LeftButton, OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_first_slider + 50, Position = midpoint },
                new OsuReplayFrame { Time = time_second_slider, Position = positionSecondSlider + new Vector2(0, 10), Actions = { OsuAction.LeftButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Ok);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
        }

        [Test]
        public void TestStacksDoNotShake()
        {
            const double time_stack_start = 1000;
            Vector2 position = new Vector2(80);

            var hitObjects = Enumerable.Range(0, 20).Select(i => new HitCircle
            {
                StartTime = time_stack_start + i * 100,
                Position = position
            }).Cast<OsuHitObject>().ToList();

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_stack_start - 450, Position = new Vector2(55), Actions = { OsuAction.LeftButton } },
            });

            addClickActionAssert(0, ClickAction.Ignore);
        }

        [Test]
        public void TestAutopilotReducesHittableRange()
        {
            const double time_circle = 1500;
            Vector2 positionCircle = Vector2.Zero;

            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_circle - 250, Position = positionCircle, Actions = { OsuAction.LeftButton } }
            }, new Mod[] { new OsuModAutopilot() });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            // note lock prevented the object from being hit, so the judgement offset should be very late.
            addJudgementOffsetAssert(hitObjects[0], referenceHitWindows.WindowFor(HitResult.Meh));
            addClickActionAssert(0, ClickAction.Shake);
        }

        [Test]
        public void TestInputDoesNotFallThroughOverlappingSliders()
        {
            const double time_first_slider = 1000;
            const double time_second_slider = 1250;
            Vector2 positionFirstSlider = new Vector2(100, 50);
            Vector2 positionSecondSlider = new Vector2(100, 80);
            var midpoint = (positionFirstSlider + positionSecondSlider) / 2;

            var hitObjects = new List<OsuHitObject>
            {
                new Slider
                {
                    StartTime = time_first_slider,
                    Position = positionFirstSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                },
                new Slider
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

            addJudgementAssert(hitObjects[0], HitResult.Ok);
            addJudgementOffsetAssert("first slider head", () => ((Slider)hitObjects[0]).HeadCircle, 0);
            addJudgementAssert(hitObjects[1], HitResult.Miss);
            // the slider head of the first slider prevents the second slider's head from being hit, so the judgement offset should be very late.
            // this is not strictly done by the hit policy implementation itself (see `OsuModClassic.blockInputToObjectsUnderSliderHead()`),
            // but we're testing this here anyways to just keep everything related to input handling and note lock in one place.
            addJudgementOffsetAssert("second slider head", () => ((Slider)hitObjects[1]).HeadCircle, referenceHitWindows.WindowFor(HitResult.Meh));
            addClickActionAssert(0, ClickAction.Hit);
        }

        [Test]
        public void TestOverlappingSlidersDontBlockEachOtherWhenFullyJudged()
        {
            const double time_first_slider = 1000;
            const double time_second_slider = 1600;
            Vector2 positionFirstSlider = new Vector2(100, 50);
            Vector2 positionSecondSlider = new Vector2(100, 80);
            var midpoint = (positionFirstSlider + positionSecondSlider) / 2;

            var hitObjects = new List<OsuHitObject>
            {
                new Slider
                {
                    StartTime = time_first_slider,
                    Position = positionFirstSlider,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(25, 0),
                    })
                },
                new Slider
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
                new OsuReplayFrame { Time = time_first_slider + 25, Position = midpoint },
                // this frame doesn't do anything on lazer, but is REQUIRED for correct playback on stable,
                // because stable during replay playback only updates game state _when it encounters a replay frame_
                new OsuReplayFrame { Time = 1250, Position = midpoint },
                new OsuReplayFrame { Time = time_second_slider + 50, Position = midpoint, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_second_slider + 75, Position = midpoint },
            });

            addJudgementAssert(hitObjects[0], HitResult.Ok);
            addJudgementOffsetAssert("first slider head", () => ((Slider)hitObjects[0]).HeadCircle, 0);
            addJudgementAssert(hitObjects[1], HitResult.Ok);
            addJudgementOffsetAssert("second slider head", () => ((Slider)hitObjects[1]).HeadCircle, 50);
            addClickActionAssert(0, ClickAction.Hit);
            addClickActionAssert(1, ClickAction.Hit);
        }

        [Test]
        public void TestOverlappingHitCirclesDontBlockEachOtherWhenBothVisible()
        {
            const double time_first_circle = 1000;
            const double time_second_circle = 1200;
            Vector2 positionFirstCircle = new Vector2(100);
            Vector2 positionSecondCircle = new Vector2(120);
            var midpoint = (positionFirstCircle + positionSecondCircle) / 2;

            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle,
                },
                new HitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle,
                },
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle, Position = midpoint, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle + 25, Position = midpoint },
                new OsuReplayFrame { Time = time_first_circle + 50, Position = midpoint, Actions = { OsuAction.RightButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 0);

            addJudgementAssert(hitObjects[1], HitResult.Meh);
            addJudgementOffsetAssert(hitObjects[1], -150);
        }

        [Test]
        public void TestOverlappingHitCirclesDontBlockEachOtherWhenFullyFadedOut()
        {
            const double time_first_circle = 1000;
            const double time_second_circle = 1200;
            const double time_third_circle = 1400;
            Vector2 positionFirstCircle = new Vector2(100);
            Vector2 positionSecondCircle = new Vector2(200);

            var hitObjects = new List<OsuHitObject>
            {
                new HitCircle
                {
                    StartTime = time_first_circle,
                    Position = positionFirstCircle,
                },
                new HitCircle
                {
                    StartTime = time_second_circle,
                    Position = positionSecondCircle,
                },
                new HitCircle
                {
                    StartTime = time_third_circle,
                    Position = positionFirstCircle,
                },
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle + 50, Position = positionFirstCircle },
                new OsuReplayFrame { Time = time_second_circle - 50, Position = positionSecondCircle },
                new OsuReplayFrame { Time = time_second_circle, Position = positionSecondCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_second_circle + 50, Position = positionSecondCircle },
                new OsuReplayFrame { Time = time_third_circle - 50, Position = positionFirstCircle },
                new OsuReplayFrame { Time = time_third_circle, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_third_circle + 50, Position = positionFirstCircle },
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 0);

            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[1], 0);

            addJudgementAssert(hitObjects[2], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[2], 0);
        }

        private void addJudgementAssert(OsuHitObject hitObject, HitResult result)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject).Type, () => Is.EqualTo(result));
        }

        private void addJudgementAssert(string name, Func<OsuHitObject?> hitObject, HitResult result)
        {
            AddAssert($"{name} judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject()).Type, () => Is.EqualTo(result));
        }

        private void addJudgementOffsetAssert(OsuHitObject hitObject, double offset)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judged at {offset}",
                () => judgementResults.Single(r => r.HitObject == hitObject).TimeOffset, () => Is.EqualTo(offset).Within(50));
        }

        private void addJudgementOffsetAssert(string name, Func<OsuHitObject?> hitObject, double offset)
        {
            AddAssert($"{name} @ judged at {offset}",
                () => judgementResults.Single(r => r.HitObject == hitObject()).TimeOffset, () => Is.EqualTo(offset).Within(50));
        }

        private void addClickActionAssert(int inputIndex, ClickAction action)
            => AddAssert($"input #{inputIndex} resulted in {action}", () => testPolicy.ClickActions[inputIndex], () => Is.EqualTo(action));

        private ScoreAccessibleReplayPlayer currentPlayer = null!;
        private List<JudgementResult> judgementResults = null!;
        private TestLegacyHitPolicy testPolicy = null!;

        private void performTest(List<OsuHitObject> hitObjects, List<ReplayFrame> frames, IEnumerable<Mod>? extraMods = null, [CallerMemberName] string testCaseName = "")
        {
            List<Mod> mods = null!;
            IBeatmap playableBeatmap = null!;
            Score score = null!;

            AddStep("set up mods", () =>
            {
                mods = new List<Mod> { new OsuModClassic() };

                if (extraMods != null)
                    mods.AddRange(extraMods);
            });

            AddStep("create beatmap", () =>
            {
                var cpi = new ControlPointInfo();
                cpi.Add(0, new TimingControlPoint { BeatLength = 1000 });
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    Metadata =
                    {
                        Title = testCaseName
                    },
                    HitObjects = hitObjects,
                    Difficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 0,
                        SliderTickRate = 3
                    },
                    BeatmapInfo =
                    {
                        Ruleset = new OsuRuleset().RulesetInfo,
                        BeatmapVersion = LegacyBeatmapEncoder.FIRST_LAZER_VERSION // for correct offset treatment by score encoder
                    },
                    ControlPointInfo = cpi
                });
                playableBeatmap = Beatmap.Value.GetPlayableBeatmap(new OsuRuleset().RulesetInfo);
            });

            AddStep("create score", () =>
            {
                score = new Score
                {
                    Replay = new Replay
                    {
                        Frames = new List<ReplayFrame>
                        {
                            // required for correct playback in stable
                            new OsuReplayFrame(0, new Vector2(256, -500)),
                            new OsuReplayFrame(0, new Vector2(256, -500))
                        }.Concat(frames).ToList()
                    },
                    ScoreInfo =
                    {
                        Ruleset = new OsuRuleset().RulesetInfo,
                        BeatmapInfo = playableBeatmap.BeatmapInfo,
                        Mods = mods.ToArray()
                    }
                };
            });

            if (exportLocation != null)
            {
                AddStep("export beatmap", () =>
                {
                    var beatmapEncoder = new LegacyBeatmapEncoder(playableBeatmap, null);

                    using (var stream = File.Open(Path.Combine(exportLocation, $"{testCaseName}.osu"), FileMode.Create))
                    {
                        var memoryStream = new MemoryStream();
                        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
                            beatmapEncoder.Encode(writer);

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(stream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        playableBeatmap.BeatmapInfo.MD5Hash = memoryStream.ComputeMD5Hash();
                    }
                });

                AddStep("export score", () =>
                {
                    using var stream = File.Open(Path.Combine(exportLocation, $"{testCaseName}.osr"), FileMode.Create);
                    var encoder = new LegacyScoreEncoder(score, playableBeatmap);
                    encoder.Encode(stream);
                });
            }

            AddStep("load player", () =>
            {
                SelectedMods.Value = mods.ToArray();

                var p = new ScoreAccessibleReplayPlayer(score);

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
            AddStep("Substitute hit policy", () =>
            {
                var playfield = currentPlayer.ChildrenOfType<OsuPlayfield>().Single();
                var currentPolicy = playfield.HitPolicy;
                playfield.HitPolicy = testPolicy = new TestLegacyHitPolicy(currentPolicy);
            });
            AddUntilStep("Wait for completion", () => currentPlayer.ScoreProcessor.HasCompleted.Value);
        }

        private class TestSpinner : Spinner
        {
            protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
            {
                base.ApplyDefaultsToSelf(controlPointInfo, difficulty);
                SpinsRequired = 1;
            }
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

        private class TestLegacyHitPolicy : LegacyHitPolicy
        {
            private readonly IHitPolicy currentPolicy;

            public TestLegacyHitPolicy(IHitPolicy currentPolicy)
            {
                this.currentPolicy = currentPolicy;
            }

            public List<ClickAction> ClickActions { get; } = new List<ClickAction>();

            public override ClickAction CheckHittable(DrawableHitObject hitObject, double time, HitResult result)
            {
                var action = currentPolicy.CheckHittable(hitObject, time, result);
                ClickActions.Add(action);
                return action;
            }
        }
    }
}
