﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Lists;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseBeatSyncedContainer : OsuTestCase
    {
        public override string Description => @"Tests beat synced containers.";

        private readonly MusicController mc;

        public TestCaseBeatSyncedContainer()
        {
            Clock = new FramedClock();
            Clock.ProcessFrame();

            Add(new BeatContainer
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });

            Add(mc = new MusicController
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            mc.ToggleVisibility();
        }

        private class BeatContainer : BeatSyncedContainer
        {
            private const int flash_layer_heigth = 150;

            private readonly InfoString timingPointCount;
            private readonly InfoString currentTimingPoint;
            private readonly InfoString beatCount;
            private readonly InfoString currentBeat;
            private readonly InfoString beatsPerMinute;
            private readonly InfoString adjustedBeatLength;
            private readonly InfoString timeUntilNextBeat;
            private readonly InfoString timeSinceLastBeat;

            private readonly Box flashLayer;

            public BeatContainer()
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
                        Margin = new MarginPadding { Bottom = flash_layer_heigth },
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
                        Height = flash_layer_heigth,
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

                Beatmap.ValueChanged += delegate
                {
                    timingPointCount.Value = 0;
                    currentTimingPoint.Value = 0;
                    beatCount.Value = 0;
                    currentBeat.Value = 0;
                    beatsPerMinute.Value = 0;
                    adjustedBeatLength.Value = 0;
                    timeUntilNextBeat.Value = 0;
                    timeSinceLastBeat.Value = 0;
                };
            }

            private SortedList<TimingControlPoint> timingPoints => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints;
            private TimingControlPoint getNextTimingPoint(TimingControlPoint current)
            {
                if (timingPoints[timingPoints.Count - 1] == current)
                    return current;

                return timingPoints[timingPoints.IndexOf(current) + 1];
            }

            private int calculateBeatCount(TimingControlPoint current)
            {
                if (timingPoints.Count == 0) return 0;

                if (timingPoints[timingPoints.Count - 1] == current)
                    return (int)Math.Ceiling((Beatmap.Value.Track.Length - current.Time) / current.BeatLength);

                return (int)Math.Ceiling((getNextTimingPoint(current).Time - current.Time) / current.BeatLength);
            }

            protected override void Update()
            {
                base.Update();
                timeUntilNextBeat.Value = TimeUntilNextBeat;
                timeSinceLastBeat.Value = TimeSinceLastBeat;
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                timingPointCount.Value = timingPoints.Count;
                currentTimingPoint.Value = timingPoints.IndexOf(timingPoint);
                beatCount.Value = calculateBeatCount(timingPoint);
                currentBeat.Value = beatIndex;
                beatsPerMinute.Value = 60000 / timingPoint.BeatLength;
                adjustedBeatLength.Value = timingPoint.BeatLength;

                flashLayer.FadeOutFromOne(timingPoint.BeatLength);
            }
        }

        private class InfoString : FillFlowContainer
        {
            private const int text_size = 20;
            private const int margin = 7;

            private readonly OsuSpriteText valueText;

            public double Value
            {
                set { valueText.Text = $"{value:G}"; }
            }

            public InfoString(string header)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Add(new OsuSpriteText { Text = header + @": ", TextSize = text_size });
                Add(valueText = new OsuSpriteText { TextSize = text_size });
                Margin = new MarginPadding(margin);
            }
        }
    }
}
