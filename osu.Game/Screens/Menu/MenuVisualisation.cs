// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using System;

namespace osu.Game.Screens.Menu
{
    internal class MenuVisualisation : Container
    {
        public override bool HandleInput => false;

        private Bindable<WorkingBeatmap> beatmap;
        private Bindable<bool> kiai = new Bindable<bool>();

        private int indexOffset = 0;
        private double timeOfLastUpdate = double.MinValue;
        private float[] audioData;

        private const int bars_per_visualizer = 250;

        public MenuVisualisation()
        {
            audioData = new float[256];
            Size = new Vector2(460);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            float barWidth = Size.X * (float)Math.Sqrt(2f * (1f - Math.Cos(MathHelper.DegreesToRadians(360f / bars_per_visualizer)))) / 2.2f;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < bars_per_visualizer; j++)
                {
                    Add(new Box()
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(barWidth, 300),
                        RelativePositionAxes = Axes.Both,
                        Scale = new Vector2(1, 0),
                        Colour = Color4.White,
                        Alpha = 0.2f,
                        Position = new Vector2(
                        -(float)Math.Sin((float)(j + i * (bars_per_visualizer / 5)) / bars_per_visualizer * 2 * MathHelper.Pi) / 2,
                        -0.5f + (float)Math.Cos((float)(j + i * (bars_per_visualizer / 5)) / bars_per_visualizer * 2 * MathHelper.Pi) / 2),
                        Rotation = 360f / bars_per_visualizer * j + 180 + 360f / 5 * i,
                    });
                }
            }

            kiai.ValueChanged += updateKiai;
        }

        private void updateKiai(bool newValue)
        {
            FadeTo(newValue ? 2.5f : 1, 75);
        }

        protected override void Update()
        {
            if (beatmap?.Value != null)
            {
                if ((bool)beatmap.Value?.Track?.IsRunning)
                {
                    ControlPoint kiaiControlPoint;
                    beatmap.Value.Beatmap.TimingInfo.TimingPointAt(beatmap.Value.Track.CurrentTime, out kiaiControlPoint);
                    kiai.Value = (kiaiControlPoint?.KiaiMode ?? false);


                    if (timeOfLastUpdate + 50 <= Clock.CurrentTime)
                    {
                        int i = 0;

                        audioData = beatmap.Value.Track.FrequencyAmplitudes;

                        foreach (Box b in Children)
                        {
                            int index = (i % bars_per_visualizer) + indexOffset;
                            if (index > 249)
                                index -= 250;

                            if (audioData[index] * 3.8f >= b.Scale.Y)
                            {
                                b.ClearTransforms(true);
                                b.Scale = new Vector2(1, audioData[index] * 3.8f);
                            }
                            i++;
                        }

                        indexOffset += 5;
                        if (indexOffset > 250)
                            indexOffset -= 250;

                        timeOfLastUpdate = Clock.CurrentTime;
                    }
                }
            }
        }

        protected override void UpdateAfterChildren()
        {
            foreach (Box b in Children)
            {
                b.ScaleTo(new Vector2(1, b.Scale.Y > 0.25f ? b.Scale.Y - 0.1f : b.Scale.Y * 0.8f), 50);

                if (b.Scale.Y > 0)
                {
                    b.Alpha = MathHelper.Clamp(b.Scale.Y - 0.01f, 0, 0.15f) * .2f / 0.15f;
                }
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap = game.Beatmap;
        }
    }
}