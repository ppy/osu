using System;
using osu.Framework;
using osu.Framework.Allocation;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Framework.Graphics.Colour;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Select
{
    class BeatmapInfoWedge : Container
    {
        private Container beatmapInfoContainer;

        private BaseGame game;

        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            this.game = game;
        }

        public void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            if (beatmap == null)
                return;

            var lastContainer = beatmapInfoContainer;

            float newDepth = lastContainer?.Depth - 1 ?? 0;

            FadeIn(250);

            BeatmapSetInfo beatmapSetInfo = beatmap.BeatmapSetInfo;
            BeatmapInfo beatmapInfo = beatmap.BeatmapInfo;

            (beatmapInfoContainer = new BufferedContainer
            {
                Depth = newDepth,
                PixelSnapping = true,
                CacheDrawnFrameBuffer = true,
                Shear = -Shear,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    // We will create the white-to-black gradient by modulating transparency and having
                    // a black backdrop. This results in an sRGB-space gradient and not linear space,
                    // transitioning from white to black more perceptually uniformly.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    // We use a container, such that we can set the colour gradient to go across the
                    // vertices of the masked container instead of the vertices of the (larger) sprite.
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColourInfo = ColourInfo.GradientVertical(Color4.White, new Color4(1f, 1f, 1f, 0.3f)),
                        Children = new []
                        {
                            // Zoomed-in and cropped beatmap background
                            new BeatmapBackgroundSprite(beatmap)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                FillMode = FillMode.Fill,
                            },
                        },
                    },
                    // Text for beatmap info
                    new FlowContainer
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Direction = FlowDirection.VerticalOnly,
                        Margin = new MarginPadding { Top = 10, Left = 25, Right = 10, Bottom = 40 },
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new SpriteText
                            {
                                Font = @"Exo2.0-MediumItalic",
                                Text = beatmapSetInfo.Metadata.Artist + " -- " + beatmapSetInfo.Metadata.Title,
                                TextSize = 28,
                                Shadow = true,
                            },
                            new SpriteText
                            {
                                Font = @"Exo2.0-MediumItalic",
                                Text = beatmapInfo.Version,
                                TextSize = 17,
                                Shadow = true,
                            },
                            new FlowContainer
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Direction = FlowDirection.HorizontalOnly,
                                AutoSizeAxes = Axes.Both,
                                Children = new []
                                {
                                    new SpriteText
                                    {
                                        Font = @"Exo2.0-Medium",
                                        Text = "mapped by ",
                                        TextSize = 15,
                                        Shadow = true,
                                    },
                                    new SpriteText
                                    {
                                        Font = @"Exo2.0-Bold",
                                        Text = beatmapSetInfo.Metadata.Author,
                                        TextSize = 15,
                                        Shadow = true,
                                    },
                                }
                            }
                        }
                    }
                }
            }).Preload(game, delegate(Drawable d)
            {
                lastContainer?.FadeOut(250);
                lastContainer?.Expire();

                Add(d);
            });
        }
    }
}
