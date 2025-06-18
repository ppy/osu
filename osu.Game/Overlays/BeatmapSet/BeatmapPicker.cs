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
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class BeatmapPicker : Container
    {
        private const float tile_icon_padding = 7;
        private const float tile_spacing = 2;

        private readonly LinkFlowContainer infoContainer;
        private readonly Statistic plays, favourites;

        public readonly DifficultiesContainer Difficulties;

        public readonly Bindable<APIBeatmap?> Beatmap = new Bindable<APIBeatmap?>();
        private APIBeatmapSet? beatmapSet;

        public APIBeatmapSet? BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;
                updateDisplay();
            }
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

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
                            OnLostHover = () => showBeatmap(Beatmap.Value, withStarRating: false),
                        },
                        infoContainer = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 11))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.BottomLeft,
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
                showBeatmap(b.NewValue, withStarRating: Difficulties.Any(d => d.IsHovered));
                updateDifficultyButtons();
            };
        }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            updateDisplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.ValueChanged += _ => updateDisplay();

            // done here so everything can bind in intialization and get the first trigger
            Beatmap.TriggerChange();
        }

        private void updateDisplay()
        {
            Difficulties.Clear();

            if (BeatmapSet != null)
            {
                Difficulties.ChildrenEnumerable = BeatmapSet.Beatmaps.Concat(BeatmapSet.Converts ?? Array.Empty<APIBeatmap>())
                                                            .Where(b => b.Ruleset.MatchesOnlineID(ruleset.Value))
                                                            .OrderBy(b => !b.Convert)
                                                            .ThenBy(b => b.StarRating)
                                                            .Select(b => new DifficultySelectorButton(b, b.Convert ? new RulesetInfo { OnlineID = 0 } : null)
                                                            {
                                                                State = DifficultySelectorState.NotSelected,
                                                                OnHovered = beatmap =>
                                                                {
                                                                    showBeatmap(beatmap, withStarRating: true);
                                                                },
                                                                OnClicked = beatmap => { Beatmap.Value = beatmap; },
                                                            });
            }

            // If a selection is already made, try and maintain it.
            if (Beatmap.Value != null)
                Beatmap.Value = Difficulties.FirstOrDefault(b => b.Beatmap.OnlineID == Beatmap.Value.OnlineID)?.Beatmap;

            // Else just choose the first available difficulty for now.
            Beatmap.Value ??= Difficulties.FirstOrDefault()?.Beatmap;

            plays.Value = BeatmapSet?.PlayCount ?? 0;
            favourites.Value = BeatmapSet?.FavouriteCount ?? 0;

            updateDifficultyButtons();
        }

        private void showBeatmap(APIBeatmap? beatmapInfo, bool withStarRating)
        {
            infoContainer.Clear();

            infoContainer.AddText(beatmapInfo?.DifficultyName ?? string.Empty, s => s.Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold));
            infoContainer.AddArbitraryDrawable(Empty().With(e => e.Width = 5));

            var beatmapOwners = beatmapInfo?.BeatmapOwners;
            bool isHostDifficulty = beatmapOwners?.Length == 1 && beatmapOwners.First().Id == beatmapSet?.AuthorID;

            if (beatmapOwners != null && !isHostDifficulty)
            {
                APIUser[] users = BeatmapSet?.RelatedUsers?.Where(u => beatmapOwners.Any(o => o.Id == u.OnlineID)).ToArray() ?? [];
                int count = users.Length;

                switch (count)
                {
                    case 0:
                        break;

                    case 1:
                        infoContainer.AddText(BeatmapsetsStrings.ShowDetailsMappedBy(string.Empty));
                        infoContainer.AddUserLink(users[0]);
                        break;

                    case 2:
                        infoContainer.AddText(BeatmapsetsStrings.ShowDetailsMappedBy(string.Empty));
                        infoContainer.AddUserLink(users[0]);
                        infoContainer.AddText(CommonStrings.ArrayAndTwoWordsConnector);
                        infoContainer.AddUserLink(users[1]);
                        break;

                    default:
                    {
                        infoContainer.AddText(BeatmapsetsStrings.ShowDetailsMappedBy(string.Empty));

                        for (int i = 0; i < count; i++)
                        {
                            infoContainer.AddUserLink(users[i]);

                            if (i < count - 2)
                                infoContainer.AddText(CommonStrings.ArrayAndWordsConnector);
                            else if (i == count - 2)
                                infoContainer.AddText(CommonStrings.ArrayAndLastWordConnector);
                        }

                        break;
                    }
                }
            }

            if (withStarRating)
            {
                infoContainer.AddArbitraryDrawable(Empty().With(e => e.Width = 5));
                infoContainer.AddText(
                    LocalisableString.Interpolate($"{BeatmapsetsStrings.ShowStatsStars} {beatmapInfo?.StarRating.FormatStarRating()}"),
                    t =>
                    {
                        t.Font = OsuFont.GetFont(size: 11, weight: FontWeight.Bold);
                        t.Colour = colours.Yellow;
                    });
            }
        }

        private void updateDifficultyButtons()
        {
            Difficulties.Children.ToList().ForEach(diff => diff.State = diff.Beatmap == Beatmap.Value ? DifficultySelectorState.Selected : DifficultySelectorState.NotSelected);
        }

        public partial class DifficultiesContainer : FillFlowContainer<DifficultySelectorButton>
        {
            public Action? OnLostHover;

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                OnLostHover?.Invoke();
            }
        }

        public partial class DifficultySelectorButton : OsuClickableContainer, IStateful<DifficultySelectorState>
        {
            private const float transition_duration = 100;
            private const float size = 54;
            private const float background_size = size - 2;

            private readonly Container background;
            private readonly Box backgroundBox;
            private readonly DifficultyIcon icon;

            public readonly APIBeatmap Beatmap;

            public Action<APIBeatmap>? OnHovered;
            public Action<APIBeatmap>? OnClicked;
            public event Action<DifficultySelectorState>? StateChanged;

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

            public DifficultySelectorButton(APIBeatmap beatmapInfo, IRulesetInfo? ruleset)
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
                    icon = new DifficultyIcon(beatmapInfo, ruleset)
                    {
                        TooltipType = DifficultyIconTooltipType.None,
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

        private partial class Statistic : FillFlowContainer
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
