// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
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
    public partial class TestSceneSliderLateHitJudgement : RateAdjustedBeatmapTestScene
    {
        // Note: In the following tests, the terminology "in range of the follow circle" is used as meaning
        // the equivalent of "in range of the follow circle as if it were in its expanded state".

        private const double time_slider_start = 1000;
        private const double time_slider_end = 1500;

        private static readonly Vector2 slider_start_position = new Vector2(256 - slider_path_length / 2, 192);
        private static readonly Vector2 slider_end_position = new Vector2(256 + slider_path_length / 2, 192);

        private ScoreAccessibleReplayPlayer currentPlayer = null!;

        private const float slider_path_length = 200;

        private readonly List<JudgementResult> judgementResults = new List<JudgementResult>();

        /// <summary>
        /// If the head circle is hit and the mouse is in range of the follow circle,
        /// then tracking should be enabled.
        /// </summary>
        [Test]
        public void TestHitLateInRangeTracks()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 100, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 100, slider_end_position, OsuAction.LeftButton),
            });

            assertHeadJudgement(HitResult.Ok);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit and the mouse is NOT in range of the follow circle,
        /// then tracking should NOT be enabled.
        /// </summary>
        [Test]
        public void TestHitLateOutOfRangeDoesNotTrack()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 100, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 100, slider_end_position, OsuAction.LeftButton),
            }, s =>
            {
                s.SliderVelocityMultiplier = 2;
            });

            assertHeadJudgement(HitResult.Ok);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit late and the mouse is in range of the follow circle,
        /// then all ticks that the follow circle has passed through should be hit.
        /// </summary>
        [Test]
        public void TestHitLateInRangeHitsTicks()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_end_position, OsuAction.LeftButton),
            }, s =>
            {
                s.TickDistanceMultiplier = 0.2f;
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(0, HitResult.LargeTickHit);
            assertTickJudgement(1, HitResult.LargeTickHit);
            assertTickJudgement(2, HitResult.LargeTickHit);
            assertTickJudgement(3, HitResult.LargeTickHit);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit late and the mouse is NOT in range of the follow circle,
        /// then all ticks that the follow circle has passed through should NOT be hit.
        /// </summary>
        [Test]
        public void TestHitLateOutOfRangeDoesNotHitTicks()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_end_position, OsuAction.LeftButton),
            }, s =>
            {
                s.SliderVelocityMultiplier = 2;
                s.TickDistanceMultiplier = 0.2f;
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(0, HitResult.LargeTickMiss);
            assertTickJudgement(1, HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is pressed after it's missed and the mouse is in range of the follow circle,
        /// then tracking should NOT be enabled.
        /// </summary>
        [Test]
        public void TestMissHeadInRangeDoesNotTrack()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 151, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 151, slider_end_position, OsuAction.LeftButton),
            }, s =>
            {
                s.TickDistanceMultiplier = 0.2f;
            });

            assertHeadJudgement(HitResult.Miss);
            assertTickJudgement(0, HitResult.LargeTickMiss);
            assertTickJudgement(1, HitResult.LargeTickMiss);
            assertTickJudgement(2, HitResult.LargeTickMiss);
            assertTickJudgement(3, HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreMiss);
        }

        /// <summary>
        /// If the head circle is hit late but after the completion of the slider and the mouse is in range of the follow circle,
        /// then all nested objects (ticks/repeats/tail) should be hit.
        /// </summary>
        [Test]
        public void TestHitLateShortSliderHitsAll()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(20, 0),
                }, 20);

                s.TickDistanceMultiplier = 0.01f;
                s.RepeatCount = 1;
            });

            assertHeadJudgement(HitResult.Meh);
            assertAllTickJudgements(HitResult.LargeTickHit);
            assertRepeatJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit late and the mouse is in range of the follow circle,
        /// then all the repeats that the follow circle has passed through should be hit.
        /// </summary>
        [Test]
        public void TestHitLateInRangeHitsRepeat()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(50, 0),
                }, 50);

                s.RepeatCount = 1;
            });

            assertHeadJudgement(HitResult.Meh);
            assertRepeatJudgement(HitResult.LargeTickHit);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit and the mouse is in range of the follow circle,
        /// then only the ticks that are in range of the cursor position should be hit.
        /// If any hitobject does not meet this criteria, ALL hitobjects after that one should be missed.
        /// </summary>
        [Test]
        public void TestHitLateDoesNotHitTicksIfAnyOutOfRange()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(70, 70),
                    new Vector2(20, 0),
                });

                s.TickDistanceMultiplier = 0.03f;
                s.SliderVelocityMultiplier = 6f;
            });

            assertHeadJudgement(HitResult.Meh);

            // At least one tick was out of range, so they all should be missed.
            assertAllTickJudgements(HitResult.LargeTickMiss);

            // This particular test actually starts tracking the slider just before the end, so the tail should be hit because of its leniency.
            assertTailJudgement(HitResult.SliderTailHit);

            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit and the mouse is in range of the follow circle,
        /// then a tick not within the follow radius from the cursor position should not be hit.
        /// </summary>
        [Test]
        public void TestHitLateInRangeDoesNotHitOutOfRangeTick()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(50, 50),
                    new Vector2(20, 0),
                });

                s.TickDistanceMultiplier = 0.3f;
                s.SliderVelocityMultiplier = 3;
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(0, HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// Same as <see cref="TestHitLateInRangeDoesNotHitOutOfRangeTick"/> except the tracking is limited to the ball
        /// because the tick was missed.
        /// </summary>
        [Test]
        public void TestHitLateInRangeDoesNotHitOutOfRangeTickAndTrackingLimitedToBall()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position, OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position, OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(50, 50),
                    new Vector2(20, 0),
                });

                s.TickDistanceMultiplier = 0.25f;
                s.SliderVelocityMultiplier = 3;
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(0, HitResult.LargeTickMiss);
            assertTickJudgement(1, HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.SliderTailHit);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// If the head circle is hit and the mouse is in range of the follow circle,
        /// then a tick not within the follow radius from the cursor position should not be hit.
        /// </summary>
        [Test]
        public void TestHitLateWithEdgeHit()
        {
            performTest(new List<ReplayFrame>
            {
                new OsuReplayFrame(time_slider_start + 150, slider_start_position - new Vector2(20), OsuAction.LeftButton),
                new OsuReplayFrame(time_slider_end + 150, slider_start_position - new Vector2(20), OsuAction.LeftButton),
            }, s =>
            {
                s.Path = new SliderPath(PathType.PERFECT_CURVE, new[]
                {
                    Vector2.Zero,
                    new Vector2(50, 50),
                    new Vector2(20, 0),
                });

                s.TickDistanceMultiplier = 0.35f;
                s.SliderVelocityMultiplier = 4;
            });

            assertHeadJudgement(HitResult.Meh);
            assertTickJudgement(0, HitResult.LargeTickMiss);
            assertTailJudgement(HitResult.IgnoreMiss);
            assertSliderJudgement(HitResult.IgnoreHit);
        }

        /// <summary>
        /// Late hit and release on each slider head of a slider stream.
        /// </summary>
        [Test]
        public void TestLateHitSliderStream()
        {
            var beatmap = new Beatmap<OsuHitObject>();

            for (int i = 0; i < 20; i++)
            {
                beatmap.HitObjects.Add(new Slider
                {
                    StartTime = time_slider_start + 75 * i, // 200BPM @ 1/4
                    Position = new Vector2(256 - slider_path_length / 2, 192),
                    TickDistanceMultiplier = 3,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(20, 0),
                    }),
                });
            }

            var replay = new List<ReplayFrame>();

            for (int i = 0; i < 20; i++)
            {
                replay.Add(new OsuReplayFrame(time_slider_start + 75 * i + 75, slider_start_position, i % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton));
                replay.Add(new OsuReplayFrame(time_slider_start + 75 * i + 140, slider_start_position));
            }

            performTest(replay, beatmap);

            AddAssert(
                $"all heads = {HitResult.Ok}",
                () => judgementResults.Where(r => r.HitObject is SliderHeadCircle).Select(r => r.Type),
                () => Has.All.EqualTo(HitResult.Ok));
        }

        private void assertHeadJudgement(HitResult result)
        {
            AddAssert(
                $"head = {result}",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderHeadCircle)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertTickJudgement(int index, HitResult result)
        {
            AddAssert(
                $"tick({index}) = {result}",
                () => judgementResults.Where(r => r.HitObject is SliderTick).ElementAtOrDefault(index)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertAllTickJudgements(HitResult result)
        {
            AddAssert(
                $"all ticks = {result}",
                () => judgementResults.Where(r => r.HitObject is SliderTick).Select(t => t.Type),
                () => Has.All.EqualTo(result));
        }

        private void assertRepeatJudgement(HitResult result)
        {
            AddAssert(
                $"repeat = {result}",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderRepeat)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertTailJudgement(HitResult result)
        {
            AddAssert(
                $"tail = {result}",
                () => judgementResults.SingleOrDefault(r => r.HitObject is SliderTailCircle)?.Type,
                () => Is.EqualTo(result));
        }

        private void assertSliderJudgement(HitResult result)
        {
            AddAssert(
                $"slider = {result}",
                () => judgementResults.SingleOrDefault(r => r.HitObject is Slider)?.Type,
                () => Is.EqualTo(result));
        }

        private void performTest(List<ReplayFrame> frames, Action<Slider>? adjustSliderFunc = null, bool classic = false)
        {
            Slider slider = new Slider
            {
                StartTime = time_slider_start,
                Position = new Vector2(256 - slider_path_length / 2, 192),
                TickDistanceMultiplier = 3,
                ClassicSliderBehaviour = classic,
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero,
                    new Vector2(slider_path_length, 0),
                }, slider_path_length),
            };

            adjustSliderFunc?.Invoke(slider);

            var beatmap = new Beatmap<OsuHitObject>
            {
                HitObjects = { slider },
                BeatmapInfo =
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        SliderMultiplier = 4,
                        SliderTickRate = 3
                    },
                    Ruleset = new OsuRuleset().RulesetInfo,
                }
            };

            performTest(frames, beatmap);
        }

        private void performTest(List<ReplayFrame> frames, Beatmap<OsuHitObject> beatmap)
        {
            beatmap.BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
            beatmap.BeatmapInfo.StackLeniency = 0;
            beatmap.BeatmapInfo.Difficulty = new BeatmapDifficulty
            {
                SliderMultiplier = 4,
                SliderTickRate = 3,
            };

            AddStep("load player", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(beatmap);

                var p = new ScoreAccessibleReplayPlayer(new Score { Replay = new Replay { Frames = frames } });

                p.OnLoadComplete += _ =>
                {
                    p.ScoreProcessor.NewJudgement += result =>
                    {
                        if (currentPlayer == p)
                            judgementResults.Add(result);

                        DrawableHitObject drawableObj = this.ChildrenOfType<DrawableHitObject>().Single(h => h.HitObject == result.HitObject);

                        var text = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Position = Content.ToLocalSpace(drawableObj.ToScreenSpace(drawableObj.OriginPosition)) - new Vector2(0, 20),
                            Text = result.IsHit ? "hit" : "miss"
                        };

                        Add(text);

                        text.FadeOutFromOne(1000).Expire();
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
