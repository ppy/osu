// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
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
            bpm.Value = BeatmapSet?.OnlineInfo?.BPM.ToLocalisableString(@"0.##") ?? (LocalisableString)"-";

            if (beatmap == null)
            {
                length.Value = string.Empty;
                circleCount.Value = string.Empty;
                sliderCount.Value = string.Empty;
            }
            else
            {
                length.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(TimeSpan.FromMilliseconds(beatmap.Length).ToFormattedDuration());
                length.Value = TimeSpan.FromMilliseconds(beatmap.Length).ToFormattedDuration();

                circleCount.Value = beatmap.OnlineInfo.CircleCount.ToLocalisableString(@"N0");
                sliderCount.Value = beatmap.OnlineInfo.SliderCount.ToLocalisableString(@"N0");
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
                    length = new Statistic(FontAwesome.Regular.Clock, BeatmapsetsStrings.ShowStatsTotalLength(string.Empty)) { Width = 0.25f },
                    bpm = new Statistic(FontAwesome.Regular.Circle, BeatmapsetsStrings.ShowStatsBpm) { Width = 0.25f },
                    circleCount = new Statistic(FontAwesome.Regular.Circle, BeatmapsetsStrings.ShowStatsCountCircles) { Width = 0.25f },
                    sliderCount = new Statistic(FontAwesome.Regular.Circle, BeatmapsetsStrings.ShowStatsCountSliders) { Width = 0.25f },
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

            public LocalisableString TooltipText { get; set; }

            public LocalisableString Value
            {
                get => value.Text;
                set => this.value.Text = value;
            }

            public Statistic(IconUsage icon, LocalisableString name)
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
