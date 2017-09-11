// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
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

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        public BeatmapPicker(BeatmapSetInfo set)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            TilesFillFlowContainer tileContainer;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        tileContainer = new TilesFillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Left = -(tile_icon_padding + tile_spacing / 2) },
                            OnLostHover = () =>
                            {
                                showBeatmap(Beatmap.Value);
                                starRating.FadeOut(100);
                            },
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
                                },
                                starRating = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    TextSize = 13,
                                    Font = @"Exo2.0-Bold",
                                    Text = "Star Difficulty",
                                    Alpha = 0,
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

            Beatmap.Value = set.Beatmaps.First();
            Beatmap.ValueChanged += showBeatmap;
            tileContainer.ChildrenEnumerable = set.Beatmaps.Select(b => new BeatmapTile(b, Beatmap)
            {
                OnHovered = beatmap =>
                {
                    showBeatmap(beatmap);
                    starRating.Text = string.Format("Star Difficulty {0:N2}", beatmap.StarDifficulty);
                    starRating.FadeIn(100);
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starRating.Colour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // done here so everything can bind in intialization and get the first trigger
            Beatmap.TriggerChange();
        }

        private void showBeatmap(BeatmapInfo beatmap) => version.Text = beatmap.Version;

        private class TilesFillFlowContainer : FillFlowContainer
        {
            public Action OnLostHover;

            protected override void OnHoverLost(InputState state)
            {
                base.OnHoverLost(state);
                OnLostHover?.Invoke();
            }
        }

        private class BeatmapTile : OsuClickableContainer
        {
            private const float transition_duration = 100;
            private const float size = 52;

            private readonly BeatmapInfo beatmap;
            private readonly Bindable<BeatmapInfo> bindable = new Bindable<BeatmapInfo>();

            private readonly Container bg;
            private readonly DifficultyIcon icon;

            public Action<BeatmapInfo> OnHovered;

            public BeatmapTile(BeatmapInfo beatmap, Bindable<BeatmapInfo> bindable)
            {
                this.beatmap = beatmap;
                this.bindable.BindTo(bindable);
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

                Action = () => this.bindable.Value = beatmap;
                this.bindable.ValueChanged += bindable_ValueChanged;
            }

            protected override bool OnHover(InputState state)
            {
                fadeIn();
                OnHovered?.Invoke(beatmap);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                if (bindable.Value != beatmap)
                    fadeOut();
            }

            private void bindable_ValueChanged(BeatmapInfo value)
            {
                if (value == beatmap)
                    fadeIn();
                else
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
