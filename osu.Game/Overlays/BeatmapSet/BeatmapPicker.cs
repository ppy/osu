// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapPicker : Container
    {
        private const float tile_icon_padding = 7;
        private const float tile_spacing = 2;

        private readonly OsuSpriteText version, starRating, starRatingText;
        private readonly FillFlowContainer starRatingContainer;
        private readonly Statistic plays, favourites;

        public readonly DifficultiesContainer Difficulties;

        public readonly Bindable<APIBeatmap> Beatmap = new Bindable<APIBeatmap>();
        private APIBeatmapSet beatmapSet;

        public APIBeatmapSet BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;
                updateDisplay();
            }
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
                        Difficulties = new DifficultiesContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -(tile_icon_padding + tile_spacing / 2), Bottom = 10 },
                            OnLostHover = () =>
                            {
                                showBeatmap(Beatmap.Value);
                                starRatingContainer.FadeOut(100);
                            },
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5f),
                            Children = new Drawable[]
                            {
                                version = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold)
                                },
                                starRatingContainer = new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Alpha = 0,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(2f, 0),
                                    Margin = new MarginPadding { Bottom = 1 },
                                    Children = new[]
                                    {
                                        starRatingText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold),
                                            Text = BeatmapsetsStrings.ShowStatsStars,
                                        },
                                        starRating = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold),
                                            Text = string.Empty,
                                        },
                                    }
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
                                plays = new Statistic(FontAwesome.Solid.PlayCircle),
                                favourites = new Statistic(FontAwesome.Solid.Heart),
                            },
                        },
                    },
                },
            };

            Beatmap.ValueChanged += b =>
            {
                showBeatmap(b.NewValue);
                updateDifficultyButtons();
            };
        }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starRating.Colour = colours.Yellow;
            starRatingText.Colour = colours.Yellow;
            updateDisplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.ValueChanged += r => updateDisplay();

            // done here so everything can bind in intialization and get the first trigger
            Beatmap.TriggerChange();
        }

        private void updateDisplay()
        {
            Difficulties.Clear();

            if (BeatmapSet != null)
            {
                Difficulties.ChildrenEnumerable = BeatmapSet.Beatmaps
                                                            .Where(b => b.Ruleset.MatchesOnlineID(ruleset.Value))
                                                            .OrderBy(b => b.StarRating)
                                                            .Select(b => new DifficultySelectorButton(b)
                                                            {
                                                                State = DifficultySelectorState.NotSelected,
                                                                OnHovered = beatmap =>
                                                                {
                                                                    showBeatmap(beatmap);
                                                                    starRating.Text = beatmap.StarRating.ToLocalisableString(@"0.##");
                                                                    starRatingContainer.FadeIn(100);
                                                                },
                                                                OnClicked = beatmap => { Beatmap.Value = beatmap; },
                                                            });
            }

            starRatingContainer.FadeOut(100);
            Beatmap.Value = Difficulties.FirstOrDefault()?.Beatmap;
            plays.Value = BeatmapSet?.PlayCount ?? 0;
            favourites.Value = BeatmapSet?.FavouriteCount ?? 0;

            updateDifficultyButtons();
        }

        private void showBeatmap(IBeatmapInfo beatmapInfo)
        {
            version.Text = beatmapInfo?.DifficultyName;
        }

        private void updateDifficultyButtons()
        {
            Difficulties.Children.ToList().ForEach(diff => diff.State = diff.Beatmap == Beatmap.Value ? DifficultySelectorState.Selected : DifficultySelectorState.NotSelected);
        }

        public class DifficultiesContainer : FillFlowContainer<DifficultySelectorButton>
        {
            public Action OnLostHover;

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                OnLostHover?.Invoke();
            }
        }

        public class DifficultySelectorButton : OsuClickableContainer, IStateful<DifficultySelectorState>
        {
            private const float transition_duration = 100;
            private const float size = 54;
            private const float background_size = size - 2;

            private readonly Container background;
            private readonly Box backgroundBox;
            private readonly DifficultyIcon icon;

            public readonly APIBeatmap Beatmap;

            public Action<APIBeatmap> OnHovered;
            public Action<APIBeatmap> OnClicked;
            public event Action<DifficultySelectorState> StateChanged;

            private DifficultySelectorState state;

            public DifficultySelectorState State
            {
                get => state;
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

            public DifficultySelectorButton(APIBeatmap beatmapInfo)
            {
                Beatmap = beatmapInfo;
                Size = new Vector2(size);
                Margin = new MarginPadding { Horizontal = tile_spacing / 2 };

                Children = new Drawable[]
                {
                    background = new Container
                    {
                        Size = new Vector2(background_size),
                        Masking = true,
                        CornerRadius = 4,
                        Child = backgroundBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.5f
                        }
                    },
                    icon = new DifficultyIcon(beatmapInfo, shouldShowTooltip: false)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(size - tile_icon_padding * 2),
                        Margin = new MarginPadding { Bottom = 1 },
                    },
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                fadeIn();
                OnHovered?.Invoke(Beatmap);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (State == DifficultySelectorState.NotSelected)
                    fadeOut();
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                OnClicked?.Invoke(Beatmap);
                return base.OnClick(e);
            }

            private void fadeIn()
            {
                background.FadeIn(transition_duration);
                icon.FadeIn(transition_duration);
            }

            private void fadeOut()
            {
                background.FadeOut();
                icon.FadeTo(0.7f, transition_duration);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                backgroundBox.Colour = colourProvider.Background6;
            }
        }

        private class Statistic : FillFlowContainer
        {
            private readonly OsuSpriteText text;

            private int value;

            public int Value
            {
                get => value;
                set
                {
                    this.value = value;
                    text.Text = Value.ToLocalisableString(@"N0");
                }
            }

            public Statistic(IconUsage icon)
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
                        Size = new Vector2(12),
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold, italics: true),
                    },
                };
            }
        }

        public enum DifficultySelectorState
        {
            Selected,
            NotSelected,
        }
    }
}
