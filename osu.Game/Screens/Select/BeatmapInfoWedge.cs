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
using System.Linq;
using osu.Game.Graphics;
using osu.Game.Beatmaps.Timing;

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

            string bpm = getBPMRange(beatmap.Beatmap);
            string length = TimeSpan.FromMilliseconds((beatmap.Beatmap.HitObjects.Last().EndTime - beatmap.Beatmap.HitObjects.First().StartTime)).ToString(@"m\:ss");
            string hitCircles = beatmap.Beatmap.HitObjects.Count(h => h.GetType().ToString().Equals("osu.Game.Modes.Osu.Objects.HitCircle")).ToString();
            string sliders = beatmap.Beatmap.HitObjects.Count(h => h.GetType().ToString().Equals("osu.Game.Modes.Osu.Objects.Slider")).ToString();

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
                        Margin = new MarginPadding { Top = 10, Left = 25, Right = 10, Bottom = 20 },
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
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
                            },
                            new FlowContainer
                            {
                                Margin = new MarginPadding { Top = 20 },
                                Spacing = new Vector2(40,0),
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    infoLabel(FontAwesome.fa_clock_o, length),
                                    infoLabel(FontAwesome.fa_circle, bpm),
                                    infoLabel(FontAwesome.fa_dot_circle_o, hitCircles),
                                    infoLabel(FontAwesome.fa_circle_o, sliders),
                                }
                            },
                        }
                    },
                }
            }).Preload(game, delegate(Drawable d)
            {
                FadeIn(250);

                lastContainer?.FadeOut(250);
                lastContainer?.Expire();

                Add(d);
            });
        }

        private string getBPMRange(Beatmap beatmap)
        {
            double bpmMax = double.MinValue;
            double bpmMin = double.MaxValue;
            double bpmMost = beatmap.BPMAt(beatmap.ControlPoints.Where(c => c.BeatLength != 0).GroupBy(c => c.BeatLength).OrderByDescending(grp => grp.Count()).First().First().Time);
            foreach (ControlPoint a in beatmap.ControlPoints)
            {
                if (a.BeatLength == 0) continue;
                double tmp = beatmap.BPMAt(a.Time);
                if (bpmMax < tmp) bpmMax = tmp;
                if (bpmMin > tmp) bpmMin = tmp;
            }
            if (bpmMax == bpmMin) return Math.Round(bpmMin) + "bpm";
            return Math.Round(bpmMin) + "-" + Math.Round(bpmMax) + "(" + Math.Round(bpmMost) + ")bpm";
        }
        
        private Container infoLabel(FontAwesome icon, string text)
        {
            return new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new[] 
                {
                    new TextAwesome
                    {
                        Icon = FontAwesome.fa_square,
                        Colour = new Color4(68,17,136,255),
                        Rotation = 45
                    },
                    new TextAwesome
                    {
                        Icon = icon,
                        Colour = new Color4(255,221,85,255),
                        Scale = new Vector2(0.8f,0.8f)
                    },
                    new SpriteText
                    {
                        Margin = new MarginPadding {Left = 13},
                        Font = @"Exo2.0-Bold",
                        Colour = new Color4(255,221,85,255),
                        Text = text,
                        TextSize = 17,
                        Origin = Anchor.CentreLeft
                    },
                }
            };
        }
    }
}
