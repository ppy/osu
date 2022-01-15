// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneBeatSyncedContainer : OsuTestScene
    {
        private TestBeatSyncedContainer beatContainer;

        private MasterGameplayClockContainer gameplayClockContainer;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Set beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            });

            AddStep("Create beat sync container", () =>
            {
                Children = new Drawable[]
                {
                    gameplayClockContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
                    {
                        Child = beatContainer = new TestBeatSyncedContainer
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                        },
                    }
                };
            });

            AddStep("Start playback", () => gameplayClockContainer.Start());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestDisallowMistimedEventFiring(bool allowMistimed)
        {
            int? lastBeatIndex = null;
            double? lastActuationTime = null;
            TimingControlPoint lastTimingPoint = null;

            AddStep($"set mistimed to {(allowMistimed ? "allowed" : "disallowed")}", () => beatContainer.AllowMistimedEventFiring = allowMistimed);

            AddStep("Set time before zero", () =>
            {
                beatContainer.NewBeat = (i, timingControlPoint, effectControlPoint, channelAmplitudes) =>
                {
                    lastActuationTime = gameplayClockContainer.CurrentTime;
                    lastTimingPoint = timingControlPoint;
                    lastBeatIndex = i;
                    beatContainer.NewBeat = null;
                };

                gameplayClockContainer.Seek(-1000);
            });

            AddUntilStep("wait for trigger", () => lastBeatIndex != null);

            if (!allowMistimed)
            {
                AddAssert("trigger is near beat length", () => lastActuationTime != null && lastBeatIndex != null && Precision.AlmostEquals(lastTimingPoint.Time + lastBeatIndex.Value * lastTimingPoint.BeatLength, lastActuationTime.Value, BeatSyncedContainer.MISTIMED_ALLOWANCE));
            }
            else
            {
                AddAssert("trigger is not near beat length", () => lastActuationTime != null && lastBeatIndex != null && !Precision.AlmostEquals(lastTimingPoint.Time + lastBeatIndex.Value * lastTimingPoint.BeatLength, lastActuationTime.Value, BeatSyncedContainer.MISTIMED_ALLOWANCE));
            }
        }

        [Test]
        public void TestNegativeBeatsStillUsingBeatmapTiming()
        {
            int? lastBeatIndex = null;
            double? lastBpm = null;

            AddStep("Set time before zero", () =>
            {
                beatContainer.NewBeat = (i, timingControlPoint, effectControlPoint, channelAmplitudes) =>
                {
                    lastBeatIndex = i;
                    lastBpm = timingControlPoint.BPM;
                };

                gameplayClockContainer.Seek(-1000);
            });

            AddUntilStep("wait for trigger", () => lastBpm != null);
            AddAssert("bpm is from beatmap", () => lastBpm != null && Precision.AlmostEquals(lastBpm.Value, 128));
            AddAssert("beat index is less than zero", () => lastBeatIndex < 0);
        }

        [Test]
        public void TestIdleBeatOnPausedClock()
        {
            double? lastBpm = null;

            AddStep("bind event", () =>
            {
                beatContainer.NewBeat = (i, timingControlPoint, effectControlPoint, channelAmplitudes) => lastBpm = timingControlPoint.BPM;
            });

            AddUntilStep("wait for trigger", () => lastBpm != null);
            AddAssert("bpm is from beatmap", () => lastBpm != null && Precision.AlmostEquals(lastBpm.Value, 128));

            AddStep("pause gameplay clock", () =>
            {
                lastBpm = null;
                gameplayClockContainer.Stop();
            });

            AddUntilStep("wait for trigger", () => lastBpm != null);
            AddAssert("bpm is default", () => lastBpm != null && Precision.AlmostEquals(lastBpm.Value, 60));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestEarlyActivationEffectPoint(bool earlyActivating)
        {
            double earlyActivationMilliseconds = earlyActivating ? 100 : 0;
            ControlPoint actualEffectPoint = null;

            AddStep($"set early activation to {earlyActivationMilliseconds}", () => beatContainer.EarlyActivationMilliseconds = earlyActivationMilliseconds);

            AddStep("seek before kiai effect point", () =>
            {
                ControlPoint expectedEffectPoint = Beatmap.Value.Beatmap.ControlPointInfo.EffectPoints.First(ep => ep.KiaiMode);
                actualEffectPoint = null;
                beatContainer.AllowMistimedEventFiring = false;

                beatContainer.NewBeat = (i, timingControlPoint, effectControlPoint, channelAmplitudes) =>
                {
                    if (Precision.AlmostEquals(gameplayClockContainer.CurrentTime + earlyActivationMilliseconds, expectedEffectPoint.Time, BeatSyncedContainer.MISTIMED_ALLOWANCE))
                        actualEffectPoint = effectControlPoint;
                };

                gameplayClockContainer.Seek(expectedEffectPoint.Time - earlyActivationMilliseconds);
            });

            AddUntilStep("wait for effect point", () => actualEffectPoint != null);

            AddAssert("effect has kiai", () => actualEffectPoint != null && ((EffectControlPoint)actualEffectPoint).KiaiMode);
        }

        private class TestBeatSyncedContainer : BeatSyncedContainer
        {
            private const int flash_layer_height = 150;

            public new bool AllowMistimedEventFiring
            {
                get => base.AllowMistimedEventFiring;
                set => base.AllowMistimedEventFiring = value;
            }

            public new double EarlyActivationMilliseconds
            {
                get => base.EarlyActivationMilliseconds;
                set => base.EarlyActivationMilliseconds = value;
            }

            private readonly InfoString timingPointCount;
            private readonly InfoString currentTimingPoint;
            private readonly InfoString beatCount;
            private readonly InfoString currentBeat;
            private readonly InfoString beatsPerMinute;
            private readonly InfoString adjustedBeatLength;
            private readonly InfoString timeUntilNextBeat;
            private readonly InfoString timeSinceLastBeat;
            private readonly InfoString currentTime;

            private readonly Box flashLayer;

            public TestBeatSyncedContainer()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = @"Info Layer",
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Bottom = flash_layer_height },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black.Opacity(150),
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    currentTime = new InfoString(@"Current time"),
                                    timingPointCount = new InfoString(@"Timing points amount"),
                                    currentTimingPoint = new InfoString(@"Current timing point"),
                                    beatCount = new InfoString(@"Beats amount (in the current timing point)"),
                                    currentBeat = new InfoString(@"Current beat"),
                                    beatsPerMinute = new InfoString(@"BPM"),
                                    adjustedBeatLength = new InfoString(@"Adjusted beat length"),
                                    timeUntilNextBeat = new InfoString(@"Time until next beat"),
                                    timeSinceLastBeat = new InfoString(@"Time since last beat"),
                                }
                            }
                        }
                    },
                    new Container
                    {
                        Name = @"Color indicator",
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = flash_layer_height,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                            },
                            flashLayer = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                                Alpha = 0,
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Beatmap.BindValueChanged(_ =>
                {
                    timingPointCount.Value = 0;
                    currentTimingPoint.Value = 0;
                    beatCount.Value = 0;
                    currentBeat.Value = 0;
                    beatsPerMinute.Value = 0;
                    adjustedBeatLength.Value = 0;
                    timeUntilNextBeat.Value = 0;
                    timeSinceLastBeat.Value = 0;
                }, true);
            }

            private List<TimingControlPoint> timingPoints => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ToList();

            private TimingControlPoint getNextTimingPoint(TimingControlPoint current)
            {
                if (timingPoints[^1] == current)
                    return current;

                int index = timingPoints.IndexOf(current); // -1 means that this is a "default beat"

                return index == -1 ? current : timingPoints[index + 1];
            }

            private int calculateBeatCount(TimingControlPoint current)
            {
                if (timingPoints.Count == 0) return 0;

                if (timingPoints[^1] == current)
                    return (int)Math.Ceiling((BeatSyncClock.CurrentTime - current.Time) / current.BeatLength);

                return (int)Math.Ceiling((getNextTimingPoint(current).Time - current.Time) / current.BeatLength);
            }

            protected override void Update()
            {
                base.Update();
                timeUntilNextBeat.Value = TimeUntilNextBeat;
                timeSinceLastBeat.Value = TimeSinceLastBeat;
                currentTime.Value = BeatSyncClock.CurrentTime;
            }

            public Action<int, TimingControlPoint, EffectControlPoint, ChannelAmplitudes> NewBeat;

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                timingPointCount.Value = timingPoints.Count;
                currentTimingPoint.Value = timingPoints.IndexOf(timingPoint);
                beatCount.Value = calculateBeatCount(timingPoint);
                currentBeat.Value = beatIndex;
                beatsPerMinute.Value = 60000 / timingPoint.BeatLength;
                adjustedBeatLength.Value = timingPoint.BeatLength;

                flashLayer.FadeOutFromOne(timingPoint.BeatLength / 4);

                NewBeat?.Invoke(beatIndex, timingPoint, effectPoint, amplitudes);
            }
        }

        private class InfoString : FillFlowContainer
        {
            private const int text_size = 20;
            private const int margin = 7;

            private readonly OsuSpriteText valueText;

            public double Value
            {
                set => valueText.Text = $"{value:0.##}";
            }

            public InfoString(string header)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Add(new OsuSpriteText { Text = header + @": ", Font = OsuFont.GetFont(size: text_size) });
                Add(valueText = new OsuSpriteText { Font = OsuFont.GetFont(size: text_size) });
                Margin = new MarginPadding(margin);
            }
        }
    }
}
