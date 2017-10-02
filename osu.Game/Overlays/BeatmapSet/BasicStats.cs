﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BasicStats : Container
    {
        private readonly Statistic length, bpm, circleCount, sliderCount;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                bpm.Value = BeatmapSet.OnlineInfo.BPM.ToString(@"0.##");
            }
        }

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (value == beatmap) return;
                beatmap = value;

                length.Value = TimeSpan.FromMilliseconds(beatmap.OnlineInfo.Length).ToString(@"m\:ss");
                circleCount.Value = beatmap.OnlineInfo.CircleCount.ToString("N0");
                sliderCount.Value = beatmap.OnlineInfo.SliderCount.ToString("N0");
            }
        }

        public BasicStats()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new[]
                {
                    length = new Statistic(FontAwesome.fa_clock_o, "Length") { Width = 0.25f },
                    bpm = new Statistic(FontAwesome.fa_circle, "BPM") { Width = 0.25f },
                    circleCount = new Statistic(FontAwesome.fa_circle_o, "Circle Count") { Width = 0.25f },
                    sliderCount = new Statistic(FontAwesome.fa_circle, "Slider Count") { Width = 0.25f },
                },
            };
        }

        private class Statistic : Container, IHasTooltip
        {
            private readonly string name;
            private readonly OsuSpriteText value;

            public string TooltipText => name;
            public string Value
            {
                get { return value.Text; }
                set { this.value.Text = value; }
            }

            public Statistic(FontAwesome icon, string name)
            {
                this.name = name;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome.fa_square,
                                Size = new Vector2(13),
                                Rotation = 45,
                                Colour = OsuColour.FromHex(@"441288"),
                            },
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                Icon = icon,
                                Size = new Vector2(13),
                                Colour = OsuColour.FromHex(@"f7dd55"),
                                Scale = new Vector2(0.8f),
                            },
                            value = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                TextSize = 13,
                                Font = @"Exo2.0-Bold",
                                Margin = new MarginPadding { Left = 10 },
                            },
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                value.Colour = colour.Yellow;
            }
        }
    }
}
