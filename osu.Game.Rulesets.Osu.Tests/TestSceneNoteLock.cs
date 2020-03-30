// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TestSceneNoteLock : RateAdjustedBeatmapTestScene
    {
        private const double early_miss_window = 1000; // time after -1000 to -500 is considered a miss
        private const double late_miss_window = 500; // time after +500 is considered a miss

        private static readonly Vector2 position_first_circle = Vector2.Zero;
        private static readonly Vector2 position_second_circle = new Vector2(80);

        /// <summary>
        /// Tests clicking the second circle before the first hitobject's start time, while the first hitobject HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleBeforeFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = position_first_circle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = position_second_circle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 100, Position = position_second_circle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Miss);
            addJudgementOffsetAssert(hitObjects[0], late_miss_window);
        }

        /// <summary>
        /// Tests clicking the second circle at the first hitobject's start time, while the first hitobject HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleAtFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = position_first_circle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = position_second_circle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle, Position = position_second_circle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 0);
        }

        /// <summary>
        /// Tests clicking the second circle after the first hitobject's start time, while the first hitobject HAS NOT been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleAfterFirstCircleTime()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = position_first_circle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = position_second_circle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle + 100, Position = position_second_circle, Actions = { OsuAction.LeftButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], 100);
        }

        /// <summary>
        /// Tests clicking the second circle before the first hitobject's start time, while the first hitobject HAS been judged.
        /// </summary>
        [Test]
        public void TestClickSecondCircleBeforeFirstCircleTimeWithFirstCircleJudged()
        {
            const double time_first_circle = 1500;
            const double time_second_circle = 1600;

            var hitObjects = new List<OsuHitObject>
            {
                new TestHitCircle
                {
                    StartTime = time_first_circle,
                    Position = position_first_circle
                },
                new TestHitCircle
                {
                    StartTime = time_second_circle,
                    Position = position_second_circle
                }
            };

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_first_circle - 200, Position = position_first_circle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle - 100, Position = position_second_circle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementOffsetAssert(hitObjects[0], -200); // time_first_circle - 200
            addJudgementOffsetAssert(hitObjects[0], -200); // time_second_circle - first_circle_time - 100
        }

        private void addJudgementAssert(OsuHitObject hitObject, HitResult result)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judgement is {result}",
                () => judgementResults.Single(r => r.HitObject == hitObject).Type == result);
        }

        private void addJudgementOffsetAssert(OsuHitObject hitObject, double offset)
        {
            AddAssert($"({hitObject.GetType().ReadableName()} @ {hitObject.StartTime}) judged at {offset}",
                () => Precision.AlmostEquals(judgementResults.Single(r => r.HitObject == hitObject).TimeOffset, offset, 100));
        }

        private ScoreAccessibleReplayPlayer currentPlayer;
        private List<JudgementResult> judgementResults;
        private bool allJudgedFired;

        private void performTest(List<OsuHitObject> hitObjects, List<ReplayFrame> frames)
        {
            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap<OsuHitObject>
                {
                    HitObjects = hitObjects,
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

        private class TestHitCircle : HitCircle
        {
            protected override HitWindows CreateHitWindows() => new TestHitWindows();
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
