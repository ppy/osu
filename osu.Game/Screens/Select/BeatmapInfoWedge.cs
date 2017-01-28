//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Game.Graphics;

namespace osu.Game.Screens.Select
{
    class BeatmapInfoWedge : Container
    {
        private static readonly Vector2 wedged_container_shear = new Vector2(0.15f, 0);

        private Container beatmapInfoContainer;

        private BaseGame game;

        public BeatmapInfoWedge()
        {
            Shear = wedged_container_shear;
            Masking = true;
            BorderColour = new Color4(221, 255, 255, 255);
            BorderThickness = 2.5f;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 15,
            };
        }

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

            float newDepth = lastContainer?.Depth + 1 ?? 0;

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
                        ColourInfo = ColourInfo.GradientVertical(Color4.White, Color4.White.Opacity(0.3f)),
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
                FadeIn(250);

                lastContainer?.FadeOut(250);
                lastContainer?.Expire();

                Add(d);
            });
        }
    }
}
