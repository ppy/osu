// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneEditorSeekSnapping : EditorClockTestScene
    {
        public TestSceneEditorSeekSnapping()
        {
            BeatDivisor.Value = 4;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var testBeatmap = new Beatmap
            {
                ControlPointInfo = new ControlPointInfo(),
                HitObjects =
                {
                    new HitCircle { StartTime = 0 },
                    new HitCircle { StartTime = 5000 }
                }
            };

            testBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 200 });
            testBeatmap.ControlPointInfo.Add(100, new TimingControlPoint { BeatLength = 400 });
            testBeatmap.ControlPointInfo.Add(175, new TimingControlPoint { BeatLength = 800 });
            testBeatmap.ControlPointInfo.Add(350, new TimingControlPoint { BeatLength = 200 });
            testBeatmap.ControlPointInfo.Add(450, new TimingControlPoint { BeatLength = 100 });
            testBeatmap.ControlPointInfo.Add(500, new TimingControlPoint { BeatLength = 307.69230769230802 });

            Beatmap.Value = CreateWorkingBeatmap(testBeatmap);

            Child = new TimingPointVisualiser(testBeatmap, 5000) { Clock = Clock };
        }

        /// <summary>
        /// Tests whether time is correctly seeked without snapping.
        /// </summary>
        [Test]
        public void TestSeekNoSnapping()
        {
            reset();

            // Forwards
            AddStep("Seek(0)", () => Clock.Seek(0));
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
            AddStep("Seek(33)", () => Clock.Seek(33));
            AddAssert("Time = 33", () => Clock.CurrentTime == 33);
            AddStep("Seek(89)", () => Clock.Seek(89));
            AddAssert("Time = 89", () => Clock.CurrentTime == 89);

            // Backwards
            AddStep("Seek(25)", () => Clock.Seek(25));
            AddAssert("Time = 25", () => Clock.CurrentTime == 25);
            AddStep("Seek(0)", () => Clock.Seek(0));
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
        }

        /// <summary>
        /// Tests whether seeking to exact beat times puts us on the beat time.
        /// These are the white/yellow ticks on the graph.
        /// </summary>
        [Test]
        public void TestSeekSnappingOnBeat()
        {
            reset();

            AddStep("Seek(0), Snap", () => Clock.SeekSnapped(0));
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
            AddStep("Seek(50), Snap", () => Clock.SeekSnapped(50));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("Seek(100), Snap", () => Clock.SeekSnapped(100));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("Seek(175), Snap", () => Clock.SeekSnapped(175));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("Seek(350), Snap", () => Clock.SeekSnapped(350));
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
            AddStep("Seek(400), Snap", () => Clock.SeekSnapped(400));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("Seek(450), Snap", () => Clock.SeekSnapped(450));
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
        }

        /// <summary>
        /// Tests whether seeking to somewhere in the middle between beats puts us on the expected beats.
        /// For example, snapping between a white/yellow beat should put us on either the yellow or white, depending on which one we're closer too.
        /// </summary>
        [Test]
        public void TestSeekSnappingInBetweenBeat()
        {
            reset();

            AddStep("Seek(24), Snap", () => Clock.SeekSnapped(24));
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
            AddStep("Seek(26), Snap", () => Clock.SeekSnapped(26));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("Seek(150), Snap", () => Clock.SeekSnapped(150));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("Seek(170), Snap", () => Clock.SeekSnapped(170));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("Seek(274), Snap", () => Clock.SeekSnapped(274));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("Seek(276), Snap", () => Clock.SeekSnapped(276));
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
        }

        /// <summary>
        /// Tests that when seeking forward with no beat snapping, beats are never explicitly snapped to, nor the next timing point (if we've skipped it).
        /// </summary>
        [Test]
        public void TestSeekForwardNoSnapping()
        {
            reset();

            AddStep("SeekForward", () => Clock.SeekForward());
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("SeekForward", () => Clock.SeekForward());
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("SeekForward", () => Clock.SeekForward());
            AddAssert("Time = 200", () => Clock.CurrentTime == 200);
            AddStep("SeekForward", () => Clock.SeekForward());
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("SeekForward", () => Clock.SeekForward());
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
        }

        /// <summary>
        /// Tests that when seeking forward with beat snapping, all beats are snapped to and timing points are never skipped.
        /// </summary>
        [Test]
        public void TestSeekForwardSnappingOnBeat()
        {
            reset();

            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
        }

        /// <summary>
        /// Tests that when seeking forward from in-between two beats, the next beat or timing point is snapped to, and no beats are skipped.
        /// This will also test being extremely close to the next beat/timing point, to ensure rounding is not an issue.
        /// </summary>
        [Test]
        public void TestSeekForwardSnappingFromInBetweenBeat()
        {
            reset();

            AddStep("Seek(49)", () => Clock.Seek(49));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("Seek(49.999)", () => Clock.Seek(49.999));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("Seek(99)", () => Clock.Seek(99));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("Seek(99.999)", () => Clock.Seek(99.999));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("Seek(174)", () => Clock.Seek(174));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("Seek(349)", () => Clock.Seek(349));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
            AddStep("Seek(399)", () => Clock.Seek(399));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("Seek(449)", () => Clock.Seek(449));
            AddStep("SeekForward, Snap", () => Clock.SeekForward(true));
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
        }

        /// <summary>
        /// Tests that when seeking backward with no beat snapping, beats are never explicitly snapped to, nor the next timing point (if we've skipped it).
        /// </summary>
        [Test]
        public void TestSeekBackwardNoSnapping()
        {
            reset();

            AddStep("Seek(450)", () => Clock.Seek(450));
            AddStep("SeekBackward", () => Clock.SeekBackward());
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("SeekBackward", () => Clock.SeekBackward());
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
            AddStep("SeekBackward", () => Clock.SeekBackward());
            AddAssert("Time = 150", () => Clock.CurrentTime == 150);
            AddStep("SeekBackward", () => Clock.SeekBackward());
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("SeekBackward", () => Clock.SeekBackward());
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
        }

        /// <summary>
        /// Tests that when seeking backward with beat snapping, all beats are snapped to and timing points are never skipped.
        /// </summary>
        [Test]
        public void TestSeekBackwardSnappingOnBeat()
        {
            reset();

            AddStep("Seek(450)", () => Clock.Seek(450));
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 350", () => Clock.CurrentTime == 350);
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 175", () => Clock.CurrentTime == 175);
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 100", () => Clock.CurrentTime == 100);
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 50", () => Clock.CurrentTime == 50);
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
        }

        /// <summary>
        /// Tests that when seeking backward from in-between two beats, the previous beat or timing point is snapped to, and no beats are skipped.
        /// This will also test being extremely close to the previous beat/timing point, to ensure rounding is not an issue.
        /// </summary>
        [Test]
        public void TestSeekBackwardSnappingFromInBetweenBeat()
        {
            reset();

            AddStep("Seek(451)", () => Clock.Seek(451));
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
            AddStep("Seek(450.999)", () => Clock.Seek(450.999));
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 450", () => Clock.CurrentTime == 450);
            AddStep("Seek(401)", () => Clock.Seek(401));
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
            AddStep("Seek(401.999)", () => Clock.Seek(401.999));
            AddStep("SeekBackward, Snap", () => Clock.SeekBackward(true));
            AddAssert("Time = 400", () => Clock.CurrentTime == 400);
        }

        /// <summary>
        /// Tests that there are no rounding issues when snapping to beats within a timing point with a floating-point beatlength.
        /// </summary>
        [Test]
        public void TestSeekingWithFloatingPointBeatLength()
        {
            reset();

            double lastTime = 0;

            AddStep("Seek(0)", () => Clock.Seek(0));

            for (int i = 0; i < 9; i++)
            {
                AddStep("SeekForward, Snap", () =>
                {
                    lastTime = Clock.CurrentTime;
                    Clock.SeekForward(true);
                });
                AddAssert("Time > lastTime", () => Clock.CurrentTime > lastTime);
            }

            for (int i = 0; i < 9; i++)
            {
                AddStep("SeekBackward, Snap", () =>
                {
                    lastTime = Clock.CurrentTime;
                    Clock.SeekBackward(true);
                });
                AddAssert("Time < lastTime", () => Clock.CurrentTime < lastTime);
            }

            AddAssert("Time = 0", () => Clock.CurrentTime == 0);
        }

        private void reset()
        {
            AddStep("Reset", () => Clock.Seek(0));
        }

        private class TimingPointVisualiser : CompositeDrawable
        {
            private readonly double length;

            private readonly Drawable tracker;

            public TimingPointVisualiser(IBeatmap beatmap, double length)
            {
                this.length = length;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Width = 0.75f;

                FillFlowContainer timelineContainer;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Name = "Background",
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(85f)
                    },
                    new Container
                    {
                        Name = "Tracks",
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(15),
                        Children = new[]
                        {
                            tracker = new Box
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Y,
                                RelativePositionAxes = Axes.X,
                                Width = 2,
                                Colour = Color4.Red,
                            },
                            timelineContainer = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(0, 5)
                            },
                        }
                    }
                };

                var timingPoints = beatmap.ControlPointInfo.TimingPoints;

                for (int i = 0; i < timingPoints.Count; i++)
                {
                    TimingControlPoint next = i == timingPoints.Count - 1 ? null : timingPoints[i + 1];
                    timelineContainer.Add(new TimingPointTimeline(timingPoints[i], next?.Time ?? length, length));
                }
            }

            protected override void Update()
            {
                base.Update();

                tracker.X = (float)(Time.Current / length);
            }

            private class TimingPointTimeline : CompositeDrawable
            {
                public TimingPointTimeline(TimingControlPoint timingPoint, double endTime, double fullDuration)
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    Box createMainTick(double time) => new Box
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomCentre,
                        RelativePositionAxes = Axes.X,
                        X = (float)(time / fullDuration),
                        Height = 10,
                        Width = 2
                    };

                    Box createBeatTick(double time) => new Box
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomCentre,
                        RelativePositionAxes = Axes.X,
                        X = (float)(time / fullDuration),
                        Height = 5,
                        Width = 2,
                        Colour = time > endTime ? Color4.Gray : Color4.Yellow
                    };

                    AddInternal(createMainTick(timingPoint.Time));
                    AddInternal(createMainTick(endTime));

                    for (double t = timingPoint.Time + timingPoint.BeatLength / 4; t < fullDuration; t += timingPoint.BeatLength / 4)
                        AddInternal(createBeatTick(t));
                }
            }
        }
    }
}
