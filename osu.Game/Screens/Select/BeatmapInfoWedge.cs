// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Select
{
    internal class BeatmapInfoWedge : OverlayContainer
    {
        private static readonly Vector2 wedged_container_shear = new Vector2(0.15f, 0);

        private Drawable beatmapInfoContainer;

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

        protected override bool HideOnEscape => false;

        protected override bool BlockPassThroughMouse => false;

        protected override void PopIn()
        {
            MoveToX(0, 800, EasingTypes.OutQuint);
            RotateTo(0, 800, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            MoveToX(-100, 800, EasingTypes.InQuint);
            RotateTo(10, 800, EasingTypes.InQuint);
        }

        public void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            if (beatmap?.BeatmapInfo == null)
            {
                State = Visibility.Hidden;
                beatmapInfoContainer?.FadeOut(250);
                beatmapInfoContainer?.Expire();
                beatmapInfoContainer = null;
                return;
            }

            State = Visibility.Visible;
            var lastContainer = beatmapInfoContainer;

            float newDepth = lastContainer?.Depth + 1 ?? 0;

            BeatmapInfo beatmapInfo = beatmap.BeatmapInfo;
            BeatmapMetadata metadata = beatmap.BeatmapInfo?.Metadata ?? beatmap.BeatmapSetInfo?.Metadata ?? new BeatmapMetadata();

            List<InfoLabel> labels = new List<InfoLabel>();

            if (beatmap.Beatmap != null)
            {
                HitObject lastObject = beatmap.Beatmap.HitObjects.LastOrDefault();
                double endTime = (lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0;

                labels.Add(new InfoLabel(new BeatmapStatistic
                {
                    Name = "Length",
                    Icon = FontAwesome.fa_clock_o,
                    Content = beatmap.Beatmap.HitObjects.Count == 0 ? "-" : TimeSpan.FromMilliseconds(endTime - beatmap.Beatmap.HitObjects.First().StartTime).ToString(@"m\:ss"),
                }));

                labels.Add(new InfoLabel(new BeatmapStatistic
                {
                    Name = "BPM",
                    Icon = FontAwesome.fa_circle,
                    Content = getBPMRange(beatmap.Beatmap),
                }));

                //get statistics fromt he current ruleset.
                labels.AddRange(beatmap.BeatmapInfo.Ruleset.CreateInstance().GetBeatmapStatistics(beatmap).Select(s => new InfoLabel(s)));
            }

            AlwaysPresent = true;

            Add(beatmapInfoContainer = new AsyncLoadWrapper(
                new BufferedContainer
                {
                    OnLoadComplete = d =>
                    {
                        FadeIn(250);

                        lastContainer?.FadeOut(250);
                        lastContainer?.Expire();
                    },
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
                            Children = new[]
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
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Direction = FillDirection.Vertical,
                            Margin = new MarginPadding { Top = 10, Left = 25, Right = 10, Bottom = 20 },
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = @"Exo2.0-MediumItalic",
                                    Text = metadata.Artist + " -- " + metadata.Title,
                                    TextSize = 28,
                                    Shadow = true,
                                },
                                new OsuSpriteText
                                {
                                    Font = @"Exo2.0-MediumItalic",
                                    Text = beatmapInfo.Version,
                                    TextSize = 17,
                                    Shadow = true,
                                },
                                new FillFlowContainer
                                {
                                    Margin = new MarginPadding { Top = 10 },
                                    Direction = FillDirection.Horizontal,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Medium",
                                            Text = "mapped by ",
                                            TextSize = 15,
                                            Shadow = true,
                                        },
                                        new OsuSpriteText
                                        {
                                            Font = @"Exo2.0-Bold",
                                            Text = metadata.Author,
                                            TextSize = 15,
                                            Shadow = true,
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Margin = new MarginPadding { Top = 20 },
                                    Spacing = new Vector2(40, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = labels
                                },
                            }
                        },
                    }
                })
            {
                Depth = newDepth,
            });
        }

        private string getBPMRange(Beatmap beatmap)
        {
            double bpmMax = beatmap.TimingInfo.BPMMaximum;
            double bpmMin = beatmap.TimingInfo.BPMMinimum;

            if (Precision.AlmostEquals(bpmMin, bpmMax)) return Math.Round(bpmMin) + "bpm";

            return Math.Round(bpmMin) + "-" + Math.Round(bpmMax) + "bpm (mostly " + Math.Round(beatmap.TimingInfo.BPMMode) + "bpm)";
        }

        public class InfoLabel : Container
        {
            public InfoLabel(BeatmapStatistic statistic)
            {
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new TextAwesome
                    {
                        Icon = FontAwesome.fa_square,
                        Origin = Anchor.Centre,
                        Colour = new Color4(68, 17, 136, 255),
                        Rotation = 45,
                        TextSize = 20
                    },
                    new TextAwesome
                    {
                        Icon = statistic.Icon,
                        Origin = Anchor.Centre,
                        Colour = new Color4(255, 221, 85, 255),
                        Scale = new Vector2(0.8f),
                        TextSize = 20
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Left = 13 },
                        Font = @"Exo2.0-Bold",
                        Colour = new Color4(255, 221, 85, 255),
                        Text = statistic.Content,
                        TextSize = 17,
                        Origin = Anchor.CentreLeft
                    },
                };
            }
        }
    }
}
