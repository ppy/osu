// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play.Pause
{
    public class PauseProgressBar : Container
    {
        private Color4 fillColour = new Color4(221, 255, 255, 255);
        private Color4 glowColour = new Color4(221, 255, 255, 150);

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

        public PauseProgressBar()
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;

            Children = new Drawable[]
            {
                new PauseProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = 35,
                    Margin = new MarginPadding
                    {
                        Bottom = 5
                    }
                },
                new Container
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Height = 5,
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
                    Height = 60,
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
                                    Height = 5,
                                    Masking = true,
                                    EdgeEffect = new EdgeEffect
                                    {
                                        Type = EdgeEffectType.Glow,
                                        Colour = glowColour,
                                        Radius = 5
                                    },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = fillColour
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
                            Height = 35,
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
                                    Width = 14,
                                    Height = 25,
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
