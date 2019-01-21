// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderInput : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Slider),
            typeof(SliderBall),
            typeof(SliderBody),
            typeof(SliderTick),
            typeof(DrawableSlider),
            typeof(DrawableSliderTick),
            typeof(DrawableRepeatPoint),
            typeof(DrawableOsuHitObject)
        };

        [SetUp]
        public void Setup()
        {
            Schedule(() => { allJudgedFired = false; });
            judgementResults = new List<JudgementResult>();
        }

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private List<JudgementResult> judgementResults;
        private bool allJudgedFired;

        public TestCaseSliderInput()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));
        }

        /// <summary>
        ///     Pressing a key before a slider, pressing the other key on the slider head, then releasing the latter pressed key
        ///     should result in tracking to end.
        ///     Frame 1 (prior to slider):          Left Click
        ///     Frame 2 (within slider hit window): Left &amp; Right Click
        ///     Frame 3 (while tracking):           Left Click
        ///     A passing test case will have the cursor lose tracking on frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRight()
        {
            AddStep("Invalid key transfer test", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 2500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 1");
            AddAssert("Tracking lost", assertMehJudge);
        }

        /// <summary>
        ///     Hitting a slider head, pressing a new key after the initial hit, then letting go of the original key used to hit
        ///     the slider should reslt in continued tracking.
        ///     Frame 1: Left Click
        ///     Frame 2: Left &amp; Right Click
        ///     Frame 3: Right Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRightThenLettingGoOfLeft()
        {
            AddStep("Left to both to right test", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 2500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 2");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        /// <summary>
        ///     Hitting a slider head, pressing a new key after the initial hit, then letting go of the new key should result
        ///     in continue tracking,
        ///     Frame 1: Left Click
        ///     Frame 2: Left &amp; Right Click
        ///     Frame 3: Left Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestTrackingRetentionLeftRightLeft()
        {
            AddStep("Tracking retention test", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = 2500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 3");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        /// <summary>
        ///     Pressing a key before a slider, pressing the other key on the slider head, then releasing the former pressed key
        ///     should result in continued tracking.
        ///     Frame 1 (prior to slider):      Left Click
        ///     Frame 2 (on slider head):       Left &amp; Right Click
        ///     Frame 3 (tracking slider body): Right Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestTrackingLeftBeforeSliderToRight()
        {
            AddStep("Tracking retention test", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.RightButton }, Time = 2500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 4");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        /// <summary>
        ///     Pressing a key before a slider and holding the slider throughout the body should result in tracking, but a miss on the slider head.
        ///     Only one frame is required:
        ///     Frame 1: Left Click
        ///     In a successful test case:
        ///     The head of the slider should be judged as a miss.
        ///     The slider end should be judged as a meh.
        /// </summary>
        [Test]
        public void TestTrackingPreclicked()
        {
            AddStep("Tracking retention test", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 5");
            AddAssert("Tracking retained, sliderhead miss", assertHeadMissTailMeh);
        }

        /// <summary>
        ///     Hitting a slider head, leaving the slider, then coming back into the slider with the same button to track it should re-start tracking.
        ///     Only one frame is required:
        ///     Frame 1: Left Click
        ///     In a successful test case:
        ///     The last tick of the slider should be judged as a great.
        /// </summary>
        [Test]
        public void TestTrackingReturnMidSlider()
        {
            AddStep("Mid-sldier tracking re-acquisition", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(150, 150), Actions = { OsuAction.LeftButton }, Time = 2000},
                    new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = 2500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3000},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 6");
            AddAssert("Tracking re-acquired", assertMidSliderJudgements);
        }

        /// <summary>
        ///     Pressing a key before a slider, hitting a slider head, leaving the slider, then coming back into the slider to track it should NOT start retracking
        ///     This is current stable behavior.
        ///     In a successful test case:
        ///     The last tick of the slider should be judged as a miss.
        /// </summary>
        [Test]
        public void TestTrackingReturnMidSliderKeyDownBefore()
        {
            AddStep("Key held down before slider, mid-slider tracking re-acquisition", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 2000},
                    new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = 2500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3000},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 7");
            AddAssert("Tracking lost", assertMidSliderJudgementFail);
        }

        /// <summary>
        ///     Halfway into a slider outside of the slider, then starting to hover the slider afterwards should result in tracking
        ///     In a successful test case:
        ///     The last tick of the slider should be judged as a great.
        /// </summary>
        [Test]
        public void TestTrackingMidSlider()
        {
            AddStep("Mid-slider new tracking acquisition", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(150, 150), Actions = { OsuAction.LeftButton }, Time = 2000},
                    new OsuReplayFrame { Position = new Vector2(200, 200), Actions = { OsuAction.LeftButton }, Time = 2500},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3000},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 8");
            AddAssert("Tracking acquired", assertMidSliderJudgements);
        }

        /// <summary>
        ///     Pressing a key before a slider, clicking another key after the slider, holding both of them and
        ///     leaving tracking, then releasing both keys, then pressing the originally pressed key should start tracking
        ///     In a successful test case:
        ///     The last tick of the slider should be judged as a great.
        /// </summary>
        [Test]
        public void TestTrackingPressBeforeSliderClickingOtherKeyLeavingSliderReleaseThenTrackOriginal()
        {
            AddStep("Mid-slider new tracking acquisition", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 250},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton, OsuAction.RightButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(100, 100), Time = 1750},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 9");
            AddAssert("Tracking acquired", assertMidSliderJudgements);
        }

        /// <summary>
        ///     Pressing a key before a slider, clicking another key after the slider, holding both of them and
        ///     leaving tracking, then releasing both keys, then pressing the originally pressed key should start tracking
        ///     In a successful test case:
        ///     The last tick of the slider should be judged as a great.
        /// </summary>
        [Test]
        public void TestClickingBeforeLeavingSliderReleasingClickingAgainThenTracking()
        {
            AddStep("Mid-slider new tracking acquisition", () =>
            {
                var frames = new List<ReplayFrame>
                {
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 1500},
                    new OsuReplayFrame { Position = new Vector2(100, 100), Actions = { OsuAction.LeftButton }, Time = 2500},
                    new OsuReplayFrame { Position = new Vector2(100, 100), Time = 2750},
                    new OsuReplayFrame { Position = new Vector2(100, 100), Actions = { OsuAction.LeftButton }, Time = 3000},
                    new OsuReplayFrame { Position = new Vector2(0, 0), Actions = { OsuAction.LeftButton }, Time = 3500},
                };

                performStaticInputTest(frames);
            });

            AddUntilStep(() => allJudgedFired, "Wait for test 10");
            AddAssert("Tracking acquired", assertMidSliderJudgements);
        }

        private bool assertMehJudge()
        {
            return judgementResults.Last().Type == HitResult.Meh;
        }

        private bool assertGreatJudge()
        {
            return judgementResults.Last().Type == HitResult.Great;
        }

        private bool assertHeadMissTailMeh()
        {
            return judgementResults.Last().Type == HitResult.Meh && judgementResults.First().Type == HitResult.Miss;
        }

        private bool assertMidSliderJudgements()
        {
            return judgementResults[judgementResults.Count - 2].Type == HitResult.Great;
        }

        private bool assertMidSliderJudgementFail()
        {
            return judgementResults[judgementResults.Count - 2].Type == HitResult.Miss;
        }

        private void performStaticInputTest(List<ReplayFrame> frames)
        {
            var slider = new Slider
            {
                StartTime = 1500,
                Position = new Vector2(0, 0),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(25, 0),
                }, 25),
            };

            // Empty frame to be added as a workaround for first frame behavior.
            // If an input exists on the first frame, the input would apply to the entire intro lead-in
            // Likely requires some discussion regarding how first frame inputs should be handled.
            frames.Insert(0, new OsuReplayFrame { Position = slider.Position, Time = 0, Actions = new List<OsuAction>() });

            Beatmap.Value = new TestWorkingBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects = { slider },
                ControlPointInfo =
                {
                    DifficultyPoints = { new DifficultyControlPoint { SpeedMultiplier = 0.1f } }
                },
                BeatmapInfo =
                {
                    BaseDifficulty = new BeatmapDifficulty { SliderTickRate = 3 },
                    Ruleset = new OsuRuleset().RulesetInfo
                },
            });

            var player = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } })
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false
            };

            Child = player;

            player.ScoreProcessor.NewJudgement += result => judgementResults.Add(result);

            player.ScoreProcessor.AllJudged += () => { allJudgedFired = true; };
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreAccessibleReplayPlayer(Score score)
                : base(score)
            {
            }
        }
    }
}
