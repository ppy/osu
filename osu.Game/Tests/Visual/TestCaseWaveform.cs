// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseWaveform : OsuTestCase
    {
        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        private readonly List<WaveformDisplay> displays = new List<WaveformDisplay>();

        public TestCaseWaveform()
        {
            MusicController mc;
            FillFlowContainer flow;
            Child = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    mc = new MusicController
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Y = 100,
                        State = Visibility.Visible
                    },
                }
            };

            for (int i = 1; i <= 16; i *= 2)
            {
                var newDisplay = new WaveformDisplay(i) { RelativeSizeAxes = Axes.Both };

                displays.Add(newDisplay);

                flow.Add(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Children = new Drawable[]
                    {
                        newDisplay,
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.75f
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"Resolution: {(1f / i).ToString("0.00")}"
                                }
                            }
                        }
                    }
                });
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            beatmapBacking.BindTo(osuGame.Beatmap);
            beatmapBacking.ValueChanged += b => b.Track.QueryWaveform(processWaveform);
        }

        private void processWaveform(Waveform waveform) => Schedule(() => displays.ForEach(d => d.Display(waveform)));

        private class WaveformDisplay : CompositeDrawable
        {
            private readonly int resolution;

            public WaveformDisplay(int resolution)
            {
                this.resolution = resolution;
            }

            public void Display(Waveform waveform)
            {
                ClearInternal();

                var generated = waveform.Generate((int)MathHelper.Clamp(Math.Ceiling(DrawWidth), 0, waveform.TotalPoints) / resolution);

                for (int i = 0; i < generated.Count; i++)
                {
                    var point = generated[i];

                    // Left channel
                    AddInternal(new NonInputBox
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.BottomLeft,
                        RelativePositionAxes = Axes.X,
                        RelativeSizeAxes = Axes.Both,
                        X = 1f / generated.Count * i,
                        Size = new Vector2(1f / generated.Count, point.Amplitude[0] / 2),
                        Colour = Color4.Red
                    });

                    if (waveform.Channels >= 2)
                    {
                        // Right channel
                        AddInternal(new NonInputBox
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.TopLeft,
                            RelativePositionAxes = Axes.X,
                            RelativeSizeAxes = Axes.Both,
                            X = 1f / generated.Count * i,
                            Size = new Vector2(1f / generated.Count, point.Amplitude[1] / 2),
                            Colour = Color4.Green
                        });
                    }
                }
            }

            private class NonInputBox : Box
            {
                public override bool HandleInput => false;
            }
        }
    }
}
