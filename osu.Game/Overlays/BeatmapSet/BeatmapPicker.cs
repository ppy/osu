// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework;
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
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapPicker : Container
    {
        private const float tile_icon_padding = 7;
        private const float tile_spacing = 2;

        private readonly DifficultiesContainer difficulties;
        private readonly OsuSpriteText version, starRating;
        private readonly Statistic plays, favourites;

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            difficulties.Clear();

            if (BeatmapSet != null)
            {
                difficulties.ChildrenEnumerable = BeatmapSet.Beatmaps.OrderBy(beatmap => beatmap.StarDifficulty).Select(b => new DifficultySelectorButton(b)
                {
                    State = DifficultySelectorState.NotSelected,
                    OnHovered = beatmap =>
                    {
                        showBeatmap(beatmap);
                        starRating.Text = beatmap.StarDifficulty.ToString("Star Difficulty 0.##");
                        starRating.FadeIn(100);
                    },
                    OnClicked = beatmap => { Beatmap.Value = beatmap; },
                });
            }

            starRating.FadeOut(100);
            Beatmap.Value = BeatmapSet?.Beatmaps.FirstOrDefault();
            plays.Value = BeatmapSet?.OnlineInfo.PlayCount ?? 0;
            favourites.Value = BeatmapSet?.OnlineInfo.FavouriteCount ?? 0;

            updateDifficultyButtons();
        }

        public BeatmapPicker()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        difficulties = new DifficultiesContainer
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
                                plays = new Statistic(FontAwesome.fa_play_circle),
                                favourites = new Statistic(FontAwesome.fa_heart),
                            },
                        },
                    },
                },
            };

            Beatmap.ValueChanged += b =>
            {
                showBeatmap(b);
                updateDifficultyButtons();
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starRating.Colour = colours.Yellow;
            updateDisplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // done here so everything can bind in intialization and get the first trigger
            Beatmap.TriggerChange();
        }

        private void showBeatmap(BeatmapInfo beatmap)
        {
            version.Text = beatmap?.Version;
        }

        private void updateDifficultyButtons()
        {
            difficulties.Children.ToList().ForEach(diff => diff.State = diff.Beatmap == Beatmap.Value ? DifficultySelectorState.Selected : DifficultySelectorState.NotSelected);
        }

        private class DifficultiesContainer : FillFlowContainer<DifficultySelectorButton>
        {
            public Action OnLostHover;

            protected override void OnHoverLost(InputState state)
            {
                base.OnHoverLost(state);
                OnLostHover?.Invoke();
            }
        }

        private class DifficultySelectorButton : OsuClickableContainer, IStateful<DifficultySelectorState>
        {
            private const float transition_duration = 100;
            private const float size = 52;

            private readonly Container bg;
            private readonly DifficultyIcon icon;

            public readonly BeatmapInfo Beatmap;

            public Action<BeatmapInfo> OnHovered;
            public Action<BeatmapInfo> OnClicked;
            public event Action<DifficultySelectorState> StateChanged;

            private DifficultySelectorState state;
            public DifficultySelectorState State
            {
                get { return state; }
                set
                {
                    if (value == state) return;
                    state = value;

                    StateChanged?.Invoke(State);
                    if (value == DifficultySelectorState.Selected)
                        fadeIn();
                    else
                        fadeOut();
                }
            }

            public DifficultySelectorButton(BeatmapInfo beatmap)
            {
                Beatmap = beatmap;
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
            }

            protected override bool OnHover(InputState state)
            {
                fadeIn();
                OnHovered?.Invoke(Beatmap);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                if (State == DifficultySelectorState.NotSelected)
                    fadeOut();
                base.OnHoverLost(state);
            }

            protected override bool OnClick(InputState state)
            {
                OnClicked?.Invoke(Beatmap);
                return base.OnClick(state);
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

            public Statistic(FontAwesome icon)
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
            }
        }

        private enum DifficultySelectorState
        {
            Selected,
            NotSelected,
        }
    }
}
