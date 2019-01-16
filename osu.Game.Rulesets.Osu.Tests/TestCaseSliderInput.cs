// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
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
        [SetUp]
        public void Setup()
        {
            Schedule(() => { allJudgedFired = false; });
        }

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private JudgementResult lastResult;
        private bool allJudgedFired;

        public TestCaseSliderInput()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));
        }

        /// <summary>
        ///     Pressing a key before a slider, pressing the other key on the slider head, then releasing the latter pressed key
        ///     should result in tracking to end.
        ///     At 250ms intervals:
        ///     Frame 1 (prior to slider):          Left Click
        ///     Frame 2 (within slider hit window): Left & Right Click
        ///     Frame 3 (while tracking):           Left Click
        ///     A passing test case will have the cursor lose tracking on frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRight()
        {
            AddStep("Invalid key transfer test", () =>
            {
                var actions = new List<List<OsuAction>>
                {
                    new List<OsuAction> { OsuAction.LeftButton, OsuAction.RightButton },
                    new List<OsuAction> { OsuAction.LeftButton }
                };

                performStaticInputTest(actions, true);
            });
            AddUntilStep(() => allJudgedFired, "Wait for test 1");
            AddAssert("Tracking lost", assertMehJudge);
        }

        /// <summary>
        ///     Hitting a slider head, pressing a new key after the initial hit, then letting go of the original key used to hit
        ///     the slider should reslt in continued tracking.
        ///     At 250ms intervals:
        ///     Frame 1: Left Click
        ///     Frame 2: Left & Right Click
        ///     Frame 3: Right Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRightThenLettingGoOfLeft()
        {
            AddStep("Left to both to right test", () =>
            {
                var actions = new List<List<OsuAction>>
                {
                    new List<OsuAction> { OsuAction.LeftButton },
                    new List<OsuAction> { OsuAction.LeftButton, OsuAction.RightButton },
                    new List<OsuAction> { OsuAction.RightButton }
                };

                performStaticInputTest(actions);
            });
            AddUntilStep(() => allJudgedFired, "Wait for test 2");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        /// <summary>
        ///     Hitting a slider head, pressing a new key after the initial hit, then letting go of the new key should result
        ///     in continue tracking,
        ///     At 250ms intervals:
        ///     Frame 1: Left Click
        ///     Frame 2: Left & Right Click
        ///     Frame 3: Right Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestTrackingRetentionLeftRightLeft()
        {
            AddStep("Tracking retention test", () =>
            {
                var actions = new List<List<OsuAction>>
                {
                    new List<OsuAction> { OsuAction.LeftButton },
                    new List<OsuAction> { OsuAction.LeftButton, OsuAction.RightButton },
                    new List<OsuAction> { OsuAction.LeftButton }
                };

                performStaticInputTest(actions);
            });
            AddUntilStep(() => allJudgedFired, "Wait for test 3");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        /// <summary>
        ///     Pressing a key before a slider, pressing the other key on the slider head, then releasing the former pressed key
        ///     should result in continued tracking.
        ///     At 250ms intervals:
        ///     Frame 1: Left Click
        ///     Frame 2: Left & Right Click
        ///     Frame 3: Right Click
        ///     A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestTrackingLeftBeforeSliderToRight()
        {
            AddStep("Tracking retention test", () =>
            {
                var actions = new List<List<OsuAction>>
                {
                    new List<OsuAction> { OsuAction.LeftButton, OsuAction.RightButton },
                    new List<OsuAction> { OsuAction.RightButton }
                };

                performStaticInputTest(actions, true);
            });
            AddUntilStep(() => allJudgedFired, "Wait for test 4");
            AddAssert("Tracking retained", assertGreatJudge);
        }

        private bool assertMehJudge()
        {
            return lastResult.Type == HitResult.Meh;
        }

        private bool assertGreatJudge()
        {
            return lastResult.Type == HitResult.Great;
        }

        private void performStaticInputTest(List<List<OsuAction>> actionsOnSlider, bool primeKey = false)
        {
            var slider = new Slider
            {
                StartTime = 1500,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(5, 0),
                }, 5),
            };

            var frames = new List<ReplayFrame>();
            double frameTime = 0;

            // Empty frame to be added as a workaround for first frame behavior.
            // If an input exists on the first frame, the input would apply to the entire intro lead-in
            // Likely requires some discussion regarding how first frame inputs should be handled.
            frames.Add(new OsuReplayFrame
            {
                Position = slider.Position,
                Time = 0,
                Actions = new List<OsuAction>()
            });

            frames.Add(new OsuReplayFrame
            {
                Position = slider.Position,
                Time = 250,
                Actions = primeKey ? new List<OsuAction> { OsuAction.LeftButton } : new List<OsuAction>()
            });

            foreach (var a in actionsOnSlider)
            {
                frames.Add(new OsuReplayFrame
                {
                    Position = slider.Position,
                    Time = slider.StartTime + frameTime,
                    Actions = a
                });
                frameTime += 250;
            }

            Beatmap.Value = new TestWorkingBeatmap(new Beatmap<OsuHitObject>
            {
                HitObjects = { slider },
                ControlPointInfo = new ControlPointInfo { DifficultyPoints = { new DifficultyControlPoint { SpeedMultiplier = 0.1f } } },
                BeatmapInfo = new BeatmapInfo { BaseDifficulty = new BeatmapDifficulty { SliderTickRate = 3 }, Ruleset = new OsuRuleset().RulesetInfo },
            });

            ScoreAccessibleReplayPlayer player = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } })
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false
            };

            Child = player;

            player.ScoreProcessor.NewJudgement += result => { lastResult = result; };

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
