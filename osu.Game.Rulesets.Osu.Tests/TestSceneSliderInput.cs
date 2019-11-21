// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderInput : RateAdjustedBeatmapTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderBall),
            typeof(DrawableSlider),
            typeof(DrawableSliderTick),
            typeof(DrawableRepeatPoint),
            typeof(DrawableOsuHitObject),
            typeof(DrawableSliderHead),
            typeof(DrawableSliderTail),
        };

        private const double time_before_slider = 250;
        private const double time_slider_start = 1500;
        private const double time_during_slide_1 = 2500;
        private const double time_during_slide_2 = 3000;
        private const double time_during_slide_3 = 3500;
        private const double time_during_slide_4 = 3800;

        private List<JudgementResult> judgementResults;
        private bool allJudgedFired;

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

            AddAssert("Tracking lost", assertMidSliderJudgementFail);
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

            AddAssert("Tracking retained", assertGreatJudge);
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

            AddAssert("Tracking retained", assertGreatJudge);
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

            AddAssert("Tracking retained", assertGreatJudge);
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

            AddAssert("Tracking retained, sliderhead miss", assertHeadMissTailTracked);
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

            AddAssert("Tracking re-acquired", assertMidSliderJudgements);
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

            AddAssert("Tracking lost", assertMidSliderJudgementFail);
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

            AddAssert("Tracking acquired", assertMidSliderJudgements);
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

            AddAssert("Tracking acquired", assertMidSliderJudgements);
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

            AddAssert("Tracking acquired", assertMidSliderJudgements);
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

            AddAssert("Tracking acquired", assertMidSliderJudgements);
        }

        private bool assertGreatJudge() => judgementResults.Last().Type == HitResult.Great;

        private bool assertHeadMissTailTracked() => judgementResults[judgementResults.Count - 2].Type == HitResult.Great && judgementResults.First().Type == HitResult.Miss;

        private bool assertMidSliderJudgements() => judgementResults[judgementResults.Count - 2].Type == HitResult.Great;

        private bool assertMidSliderJudgementFail() => judgementResults[judgementResults.Count - 2].Type == HitResult.Miss;

        private ScoreAccessibleReplayPlayer currentPlayer;

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
                            Position = new Vector2(0, 0),
                            Path = new SliderPath(PathType.PerfectCurve, new[]
                            {
                                Vector2.Zero,
                                new Vector2(25, 0),
                            }, 25),
                        }
                    },
                    BeatmapInfo =
                    {
                        BaseDifficulty = new BeatmapDifficulty { SliderTickRate = 3 },
                        Ruleset = new OsuRuleset().RulesetInfo
                    },
                });

                Beatmap.Value.Beatmap.ControlPointInfo.Add(0, new DifficultyControlPoint { SpeedMultiplier = 0.1f });

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p) judgementResults.Add(result);
                    };
                    p.ScoreProcessor.AllJudged += () =>
                    {
                        if (currentPlayer == p) allJudgedFired = true;
                    };
                };

                LoadScreen(currentPlayer = p);
                allJudgedFired = false;
                judgementResults = new List<JudgementResult>();
            });

            AddUntilStep("Beatmap at 0", () => Beatmap.Value.Track.CurrentTime == 0);
            AddUntilStep("Wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep("Wait for all judged", () => allJudgedFired);
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            protected override bool PauseOnFocusLost => false;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score, false, false)
            {
            }
        }
    }
}
