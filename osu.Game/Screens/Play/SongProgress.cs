// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using System;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private readonly int bar_height = 5;
        private readonly int graph_height = 34;
        private readonly Vector2 handle_size = new Vector2(14, 25);
        private readonly Color4 fill_colour = new Color4(221, 255, 255, 255);
        private const float transition_duration = 100;

        private SongProgressBar bar;
        private SongProgressGraph graph;

        public Action<double> OnSeek;

        private double currentTime;
        public double CurrentTime
        {
            get { return currentTime; }
            set
            {
                currentTime = value;
                updateProgress();
            }
        }

        private double duration;
        public double Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                updateProgress();
            }
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = bar_height + graph_height + handle_size.Y;

            Children = new Drawable[]
            {
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = graph_height,
                    Margin = new MarginPadding
                    {
                        Bottom = bar_height
                    }
                },
                bar = new SongProgressBar(bar_height + graph_height, handle_size, fill_colour)
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = bar_height,
                    SeekRequested = delegate (float position)
                    {
                        OnSeek?.Invoke(Duration * position);
                    }
                }
            };
        }

        public void DisplayValues(int[] values)
        {
            graph.Values = values;
        }

        private void updateProgress()
        {
            float currentProgress = (float)(CurrentTime / Duration);
            bar.UpdatePosition(currentProgress);
            graph.Progress = (int)(graph.ColumnCount * currentProgress);
        }

        protected override void PopIn()
        {
            bar.FadeTo(1f, transition_duration, EasingTypes.In);
            MoveTo(Vector2.Zero, transition_duration, EasingTypes.In);
        }

        protected override void PopOut()
        {
            bar.FadeTo(0f, transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0f, bar_height), transition_duration, EasingTypes.In);
        }
    }
}
