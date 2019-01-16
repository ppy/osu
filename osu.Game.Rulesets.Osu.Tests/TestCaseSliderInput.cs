// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        private Player player;
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
                    new List<OsuAction> { OsuAction.LeftButton },
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
                    new List<OsuAction> { OsuAction.LeftButton },
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

        private void performStaticInputTest(List<List<OsuAction>> actions, bool primeKey = false)
        {
            var sliderToAdd = new Slider
            {
                StartTime = 1500,
                Position = new Vector2(100, 100),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(5, 0),
                }, 5),
            };
            Ruleset ruleset = new OsuRuleset();
            var beatmap = createBeatmap(sliderToAdd, ruleset);
            var thisReplay = new Replay();
            var frames = new List<ReplayFrame>();
            const double test_interval = 250;
            var localIndex = 0;

            // Empty frame to be added as a workaround for first frame behavior.
            // If an input exists on the first frame, the input would apply to the entire intro lead-in
            // Likely requires some discussion regarding how first frame inputs should be handled.
            frames.Add(new OsuReplayFrame
            {
                Position = sliderToAdd.Position,
                Time = 0,
                Actions = new List<OsuAction>()
            });

            foreach (var a in actions)
            {
                // primeKey sets the first input to happen prior to the actual slider
                if (primeKey && a == actions.First())
                {
                    frames.Add(new OsuReplayFrame
                    {
                        Position = sliderToAdd.Position,
                        Time = 250,
                        Actions = a
                    });
                    localIndex++;
                }
                else
                {
                    frames.Add(new OsuReplayFrame
                    {
                        Position = sliderToAdd.Position,
                        Time = sliderToAdd.StartTime + (test_interval * (primeKey ? localIndex - 1 : localIndex)),
                        Actions = a
                    });
                    localIndex++;
                }
            }

            thisReplay.Frames = frames;
            player = loadPlayerFor(ruleset, thisReplay, beatmap);
            Child = player;

            ((ScoreAccessibleReplayPlayer)player).ScoreProcessor.NewJudgement += result =>
            {
                lastResult = result;
            };

            ((ScoreAccessibleReplayPlayer)player).ScoreProcessor.AllJudged += () =>
            {
                allJudgedFired = true;
            };
        }

        protected Player CreatePlayer(Ruleset ruleset, Score score)
        {
            return new ScoreAccessibleReplayPlayer(score)
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false
            };
        }

        private Beatmap<OsuHitObject> createBeatmap(Slider s, Ruleset r)
        {
            var b = new Beatmap<OsuHitObject>
            {
                HitObjects = { s }
            };
            b.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = 0.1f });
            b.BeatmapInfo.BaseDifficulty.SliderTickRate = 3;
            b.BeatmapInfo.Ruleset = r.RulesetInfo;
            return b;
        }

        private Player loadPlayerFor(Ruleset r, Replay rp, Beatmap<OsuHitObject> b)
        {
            var beatmap = b;
            var working = new TestWorkingBeatmap(beatmap);
            var score = new Score { Replay = rp };

            Beatmap.Value = working;

            var p = CreatePlayer(r, score);

            return p;
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
