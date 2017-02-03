using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Graphics.UserInterface
{
    public class SongProgressBar : Container
    {
        private const int bar_height = 5;
        private const int graph_height = 40;
        private const int handle_height = 25;
        private const int handle_width = 14;
        private Color4 fill_colour = new Color4(221, 255, 255, 255);
        private Color4 glow_colour = new Color4(221, 255, 255, 150);

        private Container fill;
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
                fill.Width = (float)(current.Track.CurrentTime / current.Track.Length);
            }
        }

        public SongProgressBar()
        {
            RelativeSizeAxes = Axes.X;
            Height = bar_height + graph_height + handle_height;

            Children = new Drawable[]
            {
                new SongProgressGraph
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
                new Container
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.5f
                        }
                    }
                },
                fill = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Width = 0,
                    Height = bar_height + graph_height + handle_height,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomLeft,
                            Anchor = Anchor.BottomLeft,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Origin = Anchor.BottomLeft,
                                    Anchor = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = bar_height,
                                    Masking = true,
                                    EdgeEffect = new EdgeEffect
                                    {
                                        Type = EdgeEffectType.Glow,
                                        Colour = glow_colour,
                                        Radius = 5
                                    },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = fill_colour
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Origin = Anchor.BottomRight,
                            Anchor = Anchor.BottomRight,
                            Width = 2,
                            Height = bar_height + graph_height ,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                },
                                new Container
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.TopCentre,
                                    Width = handle_width,
                                    Height = handle_height,
                                    CornerRadius = 5,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.White
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
