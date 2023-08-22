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
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneObjectOrderedHitPolicy : RateAdjustedBeatmapTestScene
    {
        private readonly OsuHitWindows referenceHitWindows;

        /// <summary>
        /// This is provided as a convenience for testing note lock behaviour against osu!stable.
        /// Setting this field to a non-null path will cause beatmap files and replays used in all test cases
        /// to be exported to disk so that they can be cross-checked against stable.
        /// </summary>
        private readonly string? exportLocation = null;

        public TestSceneObjectOrderedHitPolicy()
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
                new OsuReplayFrame { Time = time_first_circle - 200, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle - 100, Position = positionSecondCircle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Meh);
            addJudgementAssert(hitObjects[1], HitResult.Meh);
            addJudgementOffsetAssert(hitObjects[0], -200); // time_first_circle - 200
            addJudgementOffsetAssert(hitObjects[0], -200); // time_second_circle - first_circle_time - 100
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
                new OsuReplayFrame { Time = time_first_circle - 200, Position = positionFirstCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_first_circle, Position = positionSecondCircle, Actions = { OsuAction.RightButton } }
            });

            addJudgementAssert(hitObjects[0], HitResult.Meh);
            addJudgementAssert(hitObjects[1], HitResult.Ok);
            addJudgementOffsetAssert(hitObjects[0], -200); // time_first_circle - 200
            addJudgementOffsetAssert(hitObjects[1], -100); // time_second_circle - first_circle_time
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
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new Slider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.Linear, new[]
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

            addJudgementAssert(hitObjects[0], HitResult.Miss);
            addJudgementAssert(hitObjects[1], HitResult.Great);
            addJudgementAssert("slider head", () => ((Slider)hitObjects[1]).HeadCircle, HitResult.LargeTickHit);
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
                new HitCircle
                {
                    StartTime = time_circle,
                    Position = positionCircle
                },
                new Slider
                {
                    StartTime = time_slider,
                    Position = positionSlider,
                    Path = new SliderPath(PathType.Linear, new[]
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

            performTest(hitObjects, new List<ReplayFrame>
            {
                new OsuReplayFrame { Time = time_spinner - 100, Position = positionCircle, Actions = { OsuAction.LeftButton } },
                new OsuReplayFrame { Time = time_spinner + 10, Position = new Vector2(236, 192), Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_spinner + 20, Position = new Vector2(256, 172), Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_spinner + 30, Position = new Vector2(276, 192), Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_spinner + 40, Position = new Vector2(256, 212), Actions = { OsuAction.RightButton } },
                new OsuReplayFrame { Time = time_spinner + 50, Position = new Vector2(236, 192), Actions = { OsuAction.RightButton } },
            });

            addJudgementAssert(hitObjects[0], HitResult.Great);
            addJudgementAssert(hitObjects[1], HitResult.Meh);
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
                    Path = new SliderPath(PathType.Linear, new[]
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
                () => judgementResults.Single(r => r.HitObject == hitObject).TimeOffset, () => Is.EqualTo(offset).Within(100));
        }

        private ScoreAccessibleReplayPlayer currentPlayer = null!;
        private List<JudgementResult> judgementResults = null!;

        private void performTest(List<OsuHitObject> hitObjects, List<ReplayFrame> frames, [CallerMemberName] string testCaseName = "")
        {
            IBeatmap playableBeatmap = null!;
            Score score = null!;

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
                        Ruleset = new OsuRuleset().RulesetInfo
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
                        BeatmapInfo = playableBeatmap.BeatmapInfo
                    }
                };
            });

            if (exportLocation != null)
            {
                AddStep("create beatmap", () =>
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
                    var scoreToEncode = score.DeepClone();
                    scoreToEncode.Replay.Frames = scoreToEncode.Replay.Frames.Cast<OsuReplayFrame>()
                                                               .Select(frame => new OsuReplayFrame(frame.Time + LegacyBeatmapDecoder.EARLY_VERSION_TIMING_OFFSET, frame.Position, frame.Actions.ToArray()))
                                                               .ToList<ReplayFrame>();

                    using var stream = File.Open(Path.Combine(exportLocation, $"{testCaseName}.osr"), FileMode.Create);
                    var encoder = new LegacyScoreEncoder(scoreToEncode, playableBeatmap);
                    encoder.Encode(stream);
                });
            }

            AddStep("load player", () =>
            {
                SelectedMods.Value = new[] { new OsuModClassic() };

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
    }
}
