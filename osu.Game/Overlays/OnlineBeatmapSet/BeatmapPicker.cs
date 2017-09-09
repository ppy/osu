// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class BeatmapPicker : Container
    {
        private const float tile_icon_padding = 7;
        private const float tile_spacing = 2;

        private readonly OsuSpriteText version, starRating;

        public BeatmapPicker(BeatmapSetInfo set)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            FillFlowContainer tileContainer;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        tileContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -(tile_icon_padding + tile_spacing / 2) },
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Top = 10 },
                            Spacing = new Vector2(5f),
                            Children = new[]
                            {
                                version = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                    Text = "BASIC",
                                },
                                starRating = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    TextSize = 13,
                                    Font = @"Exo2.0-Bold",
                                    Text = "Star Difficulty 1.36",
                                    Margin = new MarginPadding { Bottom = 1 },
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(10f),
                            Margin = new MarginPadding { Top = 5 },
                            Children = new[]
                            {
                                new Statistic(FontAwesome.fa_play_circle, 682712),
                                new Statistic(FontAwesome.fa_heart, 357),
                            },
                        },
                    },
                },
            };

            tileContainer.ChildrenEnumerable = set.Beatmaps.Select(b => new BeatmapTile(b)
            {
                OnHovered = beatmap =>
                {
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starRating.Colour = colours.Yellow;
        }

        private class BeatmapTile : OsuClickableContainer
        {
            private const float transition_duration = 100;
            private const float size = 52;

            private readonly BeatmapInfo beatmap;

            private readonly Container bg;
            private readonly DifficultyIcon icon;

            public Action<BeatmapInfo> OnHovered;

            public BeatmapTile(BeatmapInfo beatmap)
            {
                this.beatmap = beatmap;
                Size = new Vector2(size);
                Margin = new MarginPadding { Horizontal = tile_spacing / 2 };

                Children = new Drawable[]
                {
                    bg = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                    },
                    icon = new DifficultyIcon(beatmap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(size - tile_icon_padding * 2),
                        Margin = new MarginPadding { Bottom = 1 },
                    },
                };

                fadeOut();
            }

            protected override bool OnHover(InputState state)
            {
                fadeIn();
                OnHovered?.Invoke(beatmap);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                fadeOut();
            }

            private void fadeIn()
            {
                bg.FadeIn(transition_duration);
                icon.FadeIn(transition_duration);
            }

            private void fadeOut()
            {
                bg.FadeOut();
                icon.FadeTo(0.7f, transition_duration);
            }
        }

        private class Statistic : FillFlowContainer
        {
            private readonly OsuSpriteText text;

            private int value;
            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    text.Text = Value.ToString(@"N0");
                }
            }

            public Statistic(FontAwesome icon, int value = 0)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(2f);

                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = icon,
                        Shadow = true,
                        Size = new Vector2(13),
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = @"Exo2.0-SemiBoldItalic",
                        TextSize = 14,
                    },
                };

                Value = value;
            }
        }
    }
}
