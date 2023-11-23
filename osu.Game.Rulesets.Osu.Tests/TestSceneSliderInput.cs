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
    public partial class TestSceneSliderInput : RateAdjustedBeatmapTestScene
    {
        private const double time_before_slider = 250;
        private const double time_slider_start = 1500;
        private const double time_during_slide_1 = 2500;
        private const double time_during_slide_2 = 3000;
        private const double time_during_slide_3 = 3500;
        private const double time_during_slide_4 = 3800;
        private const double time_slider_end = 4000;

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private const float slider_path_length = 25;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        [TestCase(30, 0)]
        [TestCase(30, 1)]
        [TestCase(40, 0)]
        [TestCase(40, 1)]
        [TestCase(50, 1)]
        [TestCase(60, 1)]
        [TestCase(70, 1)]
        [TestCase(80, 1)]
        [TestCase(80, 0)]
        [TestCase(80, 10)]
        [TestCase(90, 1)]
        [Ignore("headless test doesn't run at high enough precision for this to always enter a tracking state in time.")]
        public void TestVeryShortSliderMissHead(float sliderLength, int repeatCount)
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(50, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start - 10 },
                new OsuReplayFrame { Position = new Vector2(50, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start + 2000 },
            }, new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(0, 0),
                SliderVelocityMultiplier = 10f,
                RepeatCount = repeatCount,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(sliderLength, 0),
                }),
            }, 240, 1);

            AddAssert("Head judgement is first", () => judgementResults[0].HitObject is SliderHeadCircle);
            AddAssert("Tail judgement is second last", () => judgementResults[^2].HitObject is SliderTailCircle);
            AddAssert("Slider judgement is last", () => judgementResults[^1].HitObject is Slider);
        }

        // Making these too short causes breakage from frames not being processed fast enough.
        // To keep things simple, these tests are crafted to always be >16ms length.
        // If sliders shorter than this are ever used in gameplay it will probably break things and we can revisit.
        [TestCase(30, 0)]
        [TestCase(30, 1)]
        [TestCase(40, 0)]
        [TestCase(40, 1)]
        [TestCase(50, 1)]
        [TestCase(60, 1)]
        [TestCase(70, 1)]
        [TestCase(80, 1)]
        [TestCase(80, 0)]
        [TestCase(80, 10)]
        [TestCase(90, 1)]
        [Ignore("headless test doesn't run at high enough precision for this to always enter a tracking state in time.")]
        public void TestVeryShortSlider(float sliderLength, int repeatCount)
        {
            Slider slider;

            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(10, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start - 10 },
                new OsuReplayFrame { Position = new Vector2(10, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start + 2000 },
            }, slider = new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(0, 0),
                SliderVelocityMultiplier = 10f,
                RepeatCount = repeatCount,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(sliderLength, 0),
                }),
            }, 240, 1);

            assertAllMaxJudgements();

            AddAssert("Head judgement is first", () => judgementResults.First().HitObject is SliderHeadCircle);

            // Even if the last tick is hit early, the slider should always execute its final judgement at its endtime.
            // If not, hitsounds will not play on time.
            AddAssert("Judgement offset is zero", () => judgementResults.Last().TimeOffset == 0);
            AddAssert("Slider judged at end time", () => judgementResults.Last().TimeAbsolute, () => Is.EqualTo(slider.EndTime));

            AddAssert("Slider is last judgement", () => judgementResults[^1].HitObject, Is.TypeOf<Slider>);
            AddAssert("Tail is second last judgement", () => judgementResults[^2].HitObject, Is.TypeOf<SliderTailCircle>);
        }

        [TestCase(300, false)]
        [TestCase(200, true)]
        [TestCase(150, true)]
        [TestCase(120, true)]
        [TestCase(60, true)]
        [TestCase(10, true)]
        [TestCase(0, true)]
        [TestCase(-30, false)]
        [Ignore("headless test doesn't run at high enough precision for this to always enter a tracking state in time.")]
        public void TestTailLeniency(float finalPosition, bool hit)
        {
            Slider slider;

            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = Vector2.Zero, Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(finalPosition, slider_path_length * 3), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start + 20 },
            }, slider = new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(0, 0),
                SliderVelocityMultiplier = 10f,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(slider_path_length * 10, 0),
                    new Vector2(slider_path_length * 10, slider_path_length * 3),
                    new Vector2(0, slider_path_length * 3),
                }),
            }, 240, 1);

            if (hit)
                assertAllMaxJudgements();
            else
                assertMidSliderJudgementFail();

            AddAssert("Head judgement is first", () => judgementResults.First().HitObject is SliderHeadCircle);

            // Even if the last tick is hit early, the slider should always execute its final judgement at its endtime.
            // If not, hitsounds will not play on time.
            AddAssert("Judgement offset is zero", () => judgementResults.Last().TimeOffset == 0);
            AddAssert("Slider judged at end time", () => judgementResults.Last().TimeAbsolute, () => Is.EqualTo(slider.EndTime));
        }

        [Test]
        public void TestPressBothKeysSimultaneouslyAndReleaseOne()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = Vector2.Zero, Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = Vector2.Zero, Actions = { OsuAction.RightButton }, Time = time_during_slide_1 },
            });

            assertAllMaxJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before a slider starts
        /// - Press the other key on the slider head timed correctly while holding the original key
        /// - Release the latter pressed key
        /// Expected Result:
        /// A passing test case will have the cursor lose tracking on replay frame 3.
        /// </summary>
        [Test]
        public void TestInvalidKeyTransfer()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_1 },
            });

            assertMidSliderJudgementFail();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key on the slider head timed correctly
        /// - Press the other key in the middle of the slider while holding the original key
        /// - Release the original key used to hit the slider
        /// Expected Result:
        /// A passing test case will have the cursor continue tracking on replay frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRightThenLettingGoOfLeft()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = time_during_slide_2 },
            });

            assertAllMaxJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key on the slider head timed correctly
        /// - Press the other key in the middle of the slider while holding the original key
        /// - Release the new key that was pressed second
        /// Expected Result:
        /// A passing test case will have the cursor continue tracking on replay frame 3.
        /// </summary>
        [Test]
        public void TestTrackingRetentionLeftRightLeft()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = time_during_slide_1 },
            });

            assertAllMaxJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before a slider starts
        /// - Press the other key on the slider head timed correctly while holding the original key
        /// - Release the key that was held down before the slider started.
        /// Expected Result:
        /// A passing test case will have the cursor continue tracking on replay frame 3
        /// </summary>
        [Test]
        public void TestTrackingLeftBeforeSliderToRight()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = time_during_slide_1 },
            });

            assertAllMaxJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before a slider starts
        /// - Hold the key down throughout the slider without pressing any other buttons.
        /// Expected Result:
        /// A passing test case will have the cursor track the slider, but miss the slider head.
        /// </summary>
        [Test]
        public void TestTrackingPreclicked()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
            });

            assertHeadMissTailTracked();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before a slider starts
        /// - Hold the key down after the slider starts
        /// - Move the cursor away from the slider body
        /// - Move the cursor back onto the body
        /// Expected Result:
        /// A passing test case will have the cursor track the slider, miss the head, miss the ticks where its outside of the body, and resume tracking when the cursor returns.
        /// </summary>
        [Test]
        public void TestTrackingReturnMidSlider()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(150, 150), Actions = { OsuAction.LeftButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = time_during_slide_2 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_3 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_4 },
            });

            assertMidSliderJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before a slider starts
        /// - Press the other key on the slider head timed correctly while holding the original key
        /// - Release the key used to hit the slider head
        /// - While holding the first key, move the cursor away from the slider body
        /// - Still holding the first key, move the cursor back to the slider body
        /// Expected Result:
        /// A passing test case will have the slider not track despite having the cursor return to the slider body.
        /// </summary>
        [Test]
        public void TestTrackingReturnMidSliderKeyDownBefore()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = time_during_slide_2 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_3 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_4 },
            });

            assertMidSliderJudgementFail();
        }

        /// <summary>
        /// Scenario:
        /// - Wait for the slider to reach a mid-point
        /// - Press a key away from the slider body
        /// - While holding down the key, move into the slider body
        /// Expected Result:
        /// A passing test case will have the slider track the cursor after the cursor enters the slider body.
        /// </summary>
        [Test]
        public void TestTrackingMidSlider()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(150, 150), Actions = { OsuAction.LeftButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = time_during_slide_2 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_3 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_4 },
            });

            assertMidSliderJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key before the slider starts
        /// - Press another key on the slider head while holding the original key
        /// - Move out of the slider body while releasing the two pressed keys
        /// - Move back into the slider body while pressing any key.
        /// Expected Result:
        /// A passing test case will have the slider track the cursor after the cursor enters the slider body.
        /// </summary>
        [Test]
        public void TestMidSliderTrackingAcquired()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(100, 100), Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_2 },
            });

            assertMidSliderJudgements();
        }

        [Test]
        public void TestMidSliderTrackingAcquiredWithMouseDownOutsideSlider()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_before_slider },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(100, 100), Actions = { OsuAction.RightButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = time_during_slide_2 },
            });

            assertMidSliderJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key on the slider head
        /// - While holding the key, move outside of the slider body with the cursor
        /// - Release the key while outside of the slider body
        /// - Press the key again while outside of the slider body
        /// - Move back into the slider body while holding the pressed key
        /// Expected Result:
        /// A passing test case will have the slider track the cursor after the cursor enters the slider body.
        /// </summary>
        [Test]
        public void TestTrackingReleasedValidKey()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(100, 100), Actions = { OsuAction.LeftButton }, Time = time_during_slide_1 },
                new OsuReplayFrame { Position = new Vector2(100, 100), Time = time_during_slide_2 },
                new OsuReplayFrame { Position = new Vector2(100, 100), Actions = { OsuAction.LeftButton }, Time = time_during_slide_3 },
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_during_slide_4 },
            });

            assertMidSliderJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key on the slider head
        /// - While holding the key, move cursor close to the edge of tracking area
        /// - Keep the cursor on the edge of tracking area until the slider ends
        /// Expected Result:
        /// A passing test case will have the slider track the cursor throughout the whole test.
        /// </summary>
        [Test]
        public void TestTrackingAreaEdge()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, OsuHitObject.OBJECT_RADIUS * 1.19f), Actions = { OsuAction.LeftButton }, Time = time_slider_start + 250 },
                new OsuReplayFrame { Position = new Vector2(slider_path_length, OsuHitObject.OBJECT_RADIUS * 1.199f), Actions = { OsuAction.LeftButton }, Time = time_slider_end },
            });

            assertAllMaxJudgements();
        }

        /// <summary>
        /// Scenario:
        /// - Press a key on the slider head
        /// - While holding the key, move cursor just outside the tracking area
        /// - Keep the cursor just outside the tracking area until the slider ends
        /// Expected Result:
        /// A passing test case will have the slider drop the tracking on frame 2.
        /// </summary>
        [Test]
        public void TestTrackingAreaOutsideEdge()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = time_slider_start },
                new OsuReplayFrame { Position = new Vector2(0, OsuHitObject.OBJECT_RADIUS * 1.21f), Actions = { OsuAction.LeftButton }, Time = time_slider_start + 250 },
                new OsuReplayFrame { Position = new Vector2(slider_path_length, OsuHitObject.OBJECT_RADIUS * 1.201f), Actions = { OsuAction.LeftButton }, Time = time_slider_end },
            });

            assertMidSliderJudgementFail();
        }

        private void assertAllMaxJudgements()
        {
            AddAssert("All judgements max", () =>
            {
                return judgementResults.Select(j => (j.HitObject, j.Type));
            }, () => Is.EqualTo(judgementResults.Select(j => (j.HitObject, j.Judgement.MaxResult))));
        }

        private void assertHeadMissTailTracked()
        {
            AddAssert("Tracking retained", () => judgementResults[^2].Type, () => Is.EqualTo(HitResult.LargeTickHit));
            AddAssert("Slider head missed", () => judgementResults.First().IsHit, () => Is.False);
        }

        private void assertMidSliderJudgements()
        {
            AddAssert("Tracking acquired", () => judgementResults[^2].Type, () => Is.EqualTo(HitResult.LargeTickHit));
        }

        private void assertMidSliderJudgementFail()
        {
            AddAssert("Tracking lost", () => judgementResults[^2].Type, () => Is.EqualTo(HitResult.IgnoreMiss));
        }

        private void performTest(List<ReplayFrame> frames, Slider? slider = null, double? bpm = null, int? tickRate = null)
        {
            slider ??= new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(0, 0),
                SliderVelocityMultiplier = 0.1f,
                Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(slider_path_length, 0),
                }, slider_path_length),
            };

            AddStep("load player", () =>
            {
                var cpi = new ControlPointInfo();

                if (bpm != null)
                    cpi.Add(0, new TimingControlPoint { BeatLength = 60000 / bpm.Value });

                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects = { slider },
                    BeatmapInfo =
                    {
                        Difficulty = new BeatmapDifficulty { SliderTickRate = tickRate ?? 3 },
                        Ruleset = new OsuRuleset().RulesetInfo,
                    },
                    ControlPointInfo = cpi,
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
