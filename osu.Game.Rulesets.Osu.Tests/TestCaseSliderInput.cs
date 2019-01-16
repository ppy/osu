// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseSliderInput : SliderTestBase
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override List<Mod> Mods { get; set; }

        private int testID;
        private Player player;
        private JudgementResult lastResult;
        private bool allJudgedFired;

        public TestCaseSliderInput()
        {
            Mods = new List<Mod>();
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));
        }

        /// <summary>
        /// Pressing a key before a slider, pressing the other key on the slider head, then releasing the latter pressed key should result in tracking to end.
        ///
        /// At 250ms intervals:
        /// Frame 1 (prior to slider):          Left Click
        /// Frame 2 (within slider hit window): Left & Right Click
        /// Frame 3 (while tracking):           Left Click
        ///
        /// A passing test case will have the cursor lose tracking on frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRight()
        {
            AddStep("Invalid key transfer test", () => performInvalidKeyTransferTest());
            AddUntilStep(() => testID == 0 && allJudgedFired, "Wait for test 1");
            AddAssert("Tracking lost", () => assertMehJudge());
        }

        /// <summary>
        /// Pressing a key before a slider, pressing the other key on the slider head, then releasing the former pressed key should result in continued tracking.
        ///
        /// At 250ms intervals:
        /// Frame 1 (prior to slider):          Left Click
        /// Frame 2 (within slider hit window): Left & Right Click
        /// Frame 3 (while tracking):           Right Click
        ///
        /// A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestLeftBeforeSliderThenRightThenLettingGoOfLeft()
        {
            AddStep("Left to both to right test", () => performLeftToRightTransferTest());
            AddUntilStep(() => testID == 1 && allJudgedFired, "Wait for test 2");
            AddAssert("Tracking retained", () => assertGreatJudge());
        }

        /// <summary>
        /// Hitting a slider head, pressing a new key after the initial hit, then letting go of the original key used to hit the slider should reslt in continued tracking.
        ///
        /// At 250ms intervals:
        /// Frame 1: Left Click
        /// Frame 2: Left & Right Click
        /// Frame 3: Right Click
        ///
        /// A passing test case will have the cursor continue to track after frame 3.
        /// </summary>
        [Test]
        public void TestTrackingRetentionLeftRightLeft()
        {
            AddStep("Tracking retention test", () => performTrackingRetentionTest());
            AddUntilStep(() => testID == 2 && allJudgedFired, "Wait for test 3");
            AddAssert("Tracking retained", () => assertGreatJudge());
        }


        private bool assertMehJudge()
        {
            return lastResult.Type == HitResult.Meh;
        }

        private bool assertGreatJudge()
        {
            return lastResult.Type == HitResult.Great;
        }

        private void performInvalidKeyTransferTest()
        {
            List<List<OsuAction>> actions = new List<List<OsuAction>>();

            List<OsuAction> frame1 = new List<OsuAction>();
            frame1.Add(OsuAction.LeftButton);
            actions.Add(frame1);

            List<OsuAction> frame2 = new List<OsuAction>();
            frame2.Add(OsuAction.LeftButton);
            frame2.Add(OsuAction.RightButton);
            actions.Add(frame2);

            List<OsuAction> frame3 = new List<OsuAction>();
            frame3.Add(OsuAction.LeftButton);
            actions.Add(frame3);

            testID = 0;
            performStaticInputTest(actions);

        }

        private void performLeftToRightTransferTest()
        {
            List<List<OsuAction>> actions = new List<List<OsuAction>>();

            List<OsuAction> frame1 = new List<OsuAction>();
            frame1.Add(OsuAction.LeftButton);
            actions.Add(frame1);

            List<OsuAction> frame2 = new List<OsuAction>();
            frame2.Add(OsuAction.LeftButton);
            frame2.Add(OsuAction.RightButton);
            actions.Add(frame2);

            List<OsuAction> frame3 = new List<OsuAction>();
            frame3.Add(OsuAction.RightButton);
            actions.Add(frame3);

            testID = 1;
            performStaticInputTest(actions, true);
        }

        private void performTrackingRetentionTest()
        {
            List<List<OsuAction>> actions = new List<List<OsuAction>>();

            List<OsuAction> frame1 = new List<OsuAction>();
            frame1.Add(OsuAction.LeftButton);
            actions.Add(frame1);

            List<OsuAction> frame2 = new List<OsuAction>();
            frame2.Add(OsuAction.LeftButton);
            frame2.Add(OsuAction.RightButton);
            actions.Add(frame2);

            List<OsuAction> frame3 = new List<OsuAction>();
            frame3.Add(OsuAction.LeftButton);
            actions.Add(frame3);

            testID = 2;
            performStaticInputTest(actions, true);
        }

        private void performStaticInputTest(List<List<OsuAction>> actions, bool addEmptyFrame = false)
        {

            Slider sliderToAdd = CreateSlider(distance: 5, addToContent: false);
            Ruleset ruleset = new OsuRuleset();
            Beatmap<OsuHitObject> beatmap = createBeatmap(sliderToAdd, ruleset);

            Replay thisReplay = new Replay();
            List<ReplayFrame> frames = new List<ReplayFrame>();
            const double testInterval = 250;
            int localIndex = 0;

            //Empty frame to be added as a workaround for first frame behavior.
            //If an input exists on the first frame, the input would apply to the entire intro lead-in
            //Likely requires some discussion regarding how first frame inputs should be handled.
            if (addEmptyFrame)
                frames.Add(new OsuReplayFrame
                {
                    Position = sliderToAdd.Position,
                    Time = 0,
                    Actions = new List<OsuAction>()
                });

            foreach (List<OsuAction> a in actions)
            {
                frames.Add(new OsuReplayFrame
                {
                    Position = sliderToAdd.Position,
                    Time = sliderToAdd.StartTime + testInterval * localIndex,
                    Actions = actions[localIndex++]
                });
            }

            thisReplay.Frames = frames;
            Player newPlayer = loadPlayerFor(ruleset, sliderToAdd, thisReplay, beatmap);
            player = newPlayer;
            Child = player;

            ((ScoreAccessibleReplayPlayer)player).ScoreProcessor.NewJudgement += result =>
            {
                lastResult = result;
                Logger.Log(result.Type.ToString());
            };

            allJudgedFired = false;
            ((ScoreAccessibleReplayPlayer)player).ScoreProcessor.AllJudged += () =>
            {
                allJudgedFired = true;
                Logger.Log("All judged! Test ID: " + testID);
            };
        }

        protected Player CreatePlayer(Ruleset ruleset, Score score) => new ScoreAccessibleReplayPlayer(score)
        {
            AllowPause = false,
            AllowLeadIn = false,
            AllowResults = false,
        };

        private Beatmap<OsuHitObject> createBeatmap(Slider s, Ruleset r)
        {
            Beatmap<OsuHitObject> b = new Beatmap<OsuHitObject>
            {
                HitObjects = { s }
            };
            b.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = 0.1f });
            b.BeatmapInfo.BaseDifficulty.SliderTickRate = 3;
            b.BeatmapInfo.Ruleset = r.RulesetInfo;
            return b;
        }

        private Player loadPlayerFor(Ruleset r, Slider s, Replay rp, Beatmap<OsuHitObject> b)
        {
            var beatmap = b;
            var working = new TestWorkingBeatmap(beatmap);
            var score = new Score { Replay = rp };

            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { new OsuModNoFail() });
            Beatmap.Value = working;

            var player = CreatePlayer(r, score);



            return player;
        }

        private class ScoreAccessibleReplayPlayer : ReplayPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
            public new Score score => base.score;
            public ScoreAccessibleReplayPlayer(Score score)
                : base(score)
            {
            }

        }
    }
}
