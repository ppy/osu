// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using System.Collections.Generic;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        public static readonly int BAR_HEIGHT = 5;
        public static readonly int GRAPH_HEIGHT = 34;
        public static readonly Color4 FILL_COLOUR = new Color4(221, 255, 255, 255);
        public static readonly Color4 GLOW_COLOUR = new Color4(221, 255, 255, 150);
        private float progress_transition_duration = 100;

        private SongProgressBar progress;
        private SongProgressGraph graph;
        private WorkingBeatmap current;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            current = osuGame.Beatmap.Value;
        }

        protected override void Update()
        {
            base.Update();

            if (current?.TrackLoaded ?? false)
            {
                float currentProgress = (float)(current.Track.CurrentTime / current.Track.Length);

                progress.IsEnabled = true;
                progress.UpdatePosition(currentProgress);
                graph.Progress = (int)(graph.ColumnCount * currentProgress);
            }
            else
            {
                progress.IsEnabled = false;
            }
        }

        public void DisplayValues(List<int> values)
        {
            graph.Values = values;
        }

        protected override void PopIn()
        {
            progress.FadeTo(1f, progress_transition_duration, EasingTypes.In);
            MoveTo(Vector2.Zero, progress_transition_duration, EasingTypes.In);
        }

        protected override void PopOut()
        {
            progress.FadeTo(0f, progress_transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0f, BAR_HEIGHT), progress_transition_duration, EasingTypes.In);
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = BAR_HEIGHT + GRAPH_HEIGHT + SongProgressBar.HANDLE_SIZE.Y;

            Children = new Drawable[]
            {
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = GRAPH_HEIGHT,
                    Margin = new MarginPadding
                    {
                        Bottom = BAR_HEIGHT
                    }
                },
                progress = new SongProgressBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    SeekRequested = delegate (float position)
                    {
                        current?.Track?.Seek(current.Track.Length * position);
                        current?.Track?.Start();
                    }
                }
            };
        }
    }
}
