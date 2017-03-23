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
        private const int bar_height = 5;
        private readonly Vector2 handleSize = new Vector2(14, 25);
        private readonly Color4 fillColour = new Color4(221, 255, 255, 255);
        private const float transition_duration = 200;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;

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

        private double length;
        public double Length
        {
            get { return length; }
            set
            {
                length = value;
                updateProgress();
            }
        }

        public int[] Values
        {
            get { return graph.Values; }
            set { graph.Values = value; }
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = bar_height + SongProgressGraph.Column.HEIGHT + handleSize.Y;

            Children = new Drawable[]
            {
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Height = SongProgressGraph.Column.HEIGHT,
                    Margin = new MarginPadding
                    {
                        Bottom = bar_height,
                    },
                },
                bar = new SongProgressBar(bar_height, SongProgressGraph.Column.HEIGHT, handleSize, fillColour)
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    SeekRequested = delegate (float position)
                    {
                        OnSeek?.Invoke(Length * position);
                    },
                },
            };
        }

        private void updateProgress()
        {
            float currentProgress = (float)(CurrentTime / Length);
            bar.UpdatePosition(currentProgress);
            graph.Progress = (int)(graph.ColumnCount * currentProgress);
        }

        protected override void PopIn()
        {
            bar.IsEnabled = true;
            updateProgress(); //in case progress was changed while the bar was hidden

            bar.FadeIn(transition_duration, EasingTypes.In);
            MoveTo(Vector2.Zero, transition_duration, EasingTypes.In);
        }

        protected override void PopOut()
        {
            bar.IsEnabled = false;
            bar.FadeOut(transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0f, bar_height), transition_duration, EasingTypes.In);
        }
    }
}
