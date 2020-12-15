// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BasicStats : Container
    {
        private readonly Statistic length, bpm, circleCount, sliderCount;

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                updateDisplay();
            }
        }

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (value == beatmap) return;

                beatmap = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            bpm.Value = BeatmapSet?.OnlineInfo?.BPM.ToString(@"0.##") ?? "-";

            if (beatmap == null)
            {
                length.Value = string.Empty;
                circleCount.Value = string.Empty;
                sliderCount.Value = string.Empty;
            }
            else
            {
                length.Value = TimeSpan.FromMilliseconds(beatmap.Length).ToString(@"m\:ss");
                circleCount.Value = beatmap.OnlineInfo.CircleCount.ToString();
                sliderCount.Value = beatmap.OnlineInfo.SliderCount.ToString();
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
                    length = new Statistic(FontAwesome.Regular.Clock, "Length") { Width = 0.25f },
                    bpm = new Statistic(FontAwesome.Regular.Circle, "BPM") { Width = 0.25f },
                    circleCount = new Statistic(FontAwesome.Regular.Circle, "Circle Count") { Width = 0.25f },
                    sliderCount = new Statistic(FontAwesome.Regular.Circle, "Slider Count") { Width = 0.25f },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateDisplay();
        }

        private class Statistic : Container, IHasTooltip
        {
            private readonly OsuSpriteText value;

            public string TooltipText { get; }

            public string Value
            {
                get => value.Text;
                set => this.value.Text = value;
            }

            public Statistic(IconUsage icon, string name)
            {
                TooltipText = name;
                RelativeSizeAxes = Axes.X;
                Height = 24f;

                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome.Solid.Square,
                                Size = new Vector2(12),
                                Rotation = 45,
                                Colour = Color4Extensions.FromHex(@"441288"),
                            },
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                Icon = icon,
                                Size = new Vector2(12),
                                Colour = Color4Extensions.FromHex(@"f7dd55"),
                                Scale = new Vector2(0.8f),
                            },
                            value = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 10 },
                                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
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
