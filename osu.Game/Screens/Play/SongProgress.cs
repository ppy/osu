// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public class SongProgress : Container
    {
        private const int graph_height = 34;
        private const int handle_height = 25;
        private const int handle_width = 14;
        public static readonly Color4 FILL_COLOUR = new Color4(221, 255, 255, 255);
        public static readonly Color4 GLOW_COLOUR = new Color4(221, 255, 255, 150);

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

                progress.UpdatePosition(currentProgress);
                graph.Progress = (int)(graph.ColumnCount * currentProgress);
            }
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = SongProgressBar.BAR_HEIGHT + graph_height + handle_height;

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
                        Bottom = SongProgressBar.BAR_HEIGHT
                    }
                },
                progress = new SongProgressBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    IsEnabled = true,
                    SeekRequested = delegate (float position)
                    {
                        Framework.Logging.Logger.Log($@"Seeked to {position}");
                    }
                }

                //handle = new Container
                //        {
                //            Origin = Anchor.BottomLeft,
                //            Anchor = Anchor.BottomLeft,
                //            Width = 2,
                //            Height = bar_height + graph_height,
                //            Position = new Vector2(2, 0),
                //            Children = new Drawable[]
                //            {
                //                new Box
                //                {
                //                    RelativeSizeAxes = Axes.Both,
                //                    Colour = Color4.White
                //                },
                //                new Container
                //                {
                //                    Origin = Anchor.BottomCentre,
                //                    Anchor = Anchor.TopCentre,
                //                    Width = handle_width,
                //                    Height = handle_height,
                //                    CornerRadius = 5,
                //                    Masking = true,
                //                    Children = new Drawable[]
                //                    {
                //                        new Box
                //                        {
                //                            RelativeSizeAxes = Axes.Both,
                //                            Colour = Color4.White
                //                        }
                //                    }
                //                }
                //            }
                //        }
            };
        }
    }
}
