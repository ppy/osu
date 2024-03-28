// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class FilterControl : Container
    {
        public const float HEIGHT = 2 * side_margin + 120;

        private const float side_margin = 10;

        public Action<FilterCriteria> FilterChanged;

        public Bindable<string> CurrentTextSearch => searchTextBox.Current;

        public LocalisableString InformationalText
        {
            get => searchTextBox.FilterText.Text;
            set => searchTextBox.FilterText.Text = value;
        }

        private OsuTabControl<SortMode> sortTabs;

        private Bindable<SortMode> sortMode;

        private Bindable<GroupMode> groupMode;

        private FilterControlTextBox searchTextBox;

        private CollectionDropdown collectionDropdown;

        public FilterCriteria CreateCriteria()
        {
            string query = searchTextBox.Text;

            var criteria = new FilterCriteria
            {
                Group = groupMode.Value,
                Sort = sortMode.Value,
                AllowConvertedBeatmaps = showConverted.Value,
                Ruleset = ruleset.Value,
                Mods = mods.Value,
                CollectionBeatmapMD5Hashes = collectionDropdown.Current.Value?.Collection?.PerformRead(c => c.BeatmapMD5Hashes).ToImmutableHashSet()
            };

            if (!minimumStars.IsDefault)
                criteria.UserStarDifficulty.Min = minimumStars.Value;

            if (!maximumStars.IsDefault)
                criteria.UserStarDifficulty.Max = maximumStars.Value;

            criteria.RulesetCriteria = ruleset.Value.CreateInstance().CreateRulesetFilterCriteria();

            FilterQueryParser.ApplyQueries(criteria, query);
            return criteria;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            base.ReceivePositionalInputAt(screenSpacePos) || sortTabs.ReceivePositionalInputAt(screenSpacePos);

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, OsuConfigManager config)
        {
            sortMode = config.GetBindable<SortMode>(OsuSetting.SongSelectSortingMode);
            groupMode = config.GetBindable<GroupMode>(OsuSetting.SongSelectGroupingMode);

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    Width = 2,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Padding = new MarginPadding(side_margin),
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    // Reverse ChildID so that dropdowns in the top section appear on top of the bottom section.
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        RelativeSizeAxes = Axes.Both,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            searchTextBox = new FilterControlTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 1,
                                Colour = OsuColour.Gray(80),
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(GridSizeMode.Absolute, OsuTabControl<SortMode>.HORIZONTAL_SPACING),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, OsuTabControl<SortMode>.HORIZONTAL_SPACING),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = SortStrings.Default,
                                            Font = OsuFont.GetFont(size: 14),
                                            Margin = new MarginPadding(5),
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                        },
                                        Empty(),
                                        sortTabs = new OsuTabControl<SortMode>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 24,
                                            AutoSort = true,
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            AccentColour = colours.GreenLight,
                                            Current = { BindTarget = sortMode }
                                        },
                                        Empty(),
                                        new OsuTabControlCheckbox
                                        {
                                            Text = "Show converted",
                                            Current = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                        },
                                    }
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 40,
                                Children = new Drawable[]
                                {
                                    new RangeSlider
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Label = "Difficulty range",
                                        LowerBound = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                                        UpperBound = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                                        RelativeSizeAxes = Axes.Both,
                                        Width = 0.48f,
                                        DefaultStringLowerBound = "0",
                                        DefaultStringUpperBound = "∞",
                                        DefaultTooltipUpperBound = UserInterfaceStrings.NoLimit,
                                        TooltipSuffix = "stars"
                                    },
                                    collectionDropdown = new CollectionDropdown
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        RequestFilter = updateCriteria,
                                        RelativeSizeAxes = Axes.X,
                                        Y = 4,
                                        Width = 0.5f,
                                    }
                                }
                            },
                        }
                    }
                }
            };

            config.BindWith(OsuSetting.ShowConvertedBeatmaps, showConverted);
            showConverted.ValueChanged += _ => updateCriteria();

            config.BindWith(OsuSetting.DisplayStarsMinimum, minimumStars);
            minimumStars.ValueChanged += _ => updateCriteria();

            config.BindWith(OsuSetting.DisplayStarsMaximum, maximumStars);
            maximumStars.ValueChanged += _ => updateCriteria();

            ruleset.BindValueChanged(_ => updateCriteria());
            mods.BindValueChanged(_ => updateCriteria());

            groupMode.BindValueChanged(_ => updateCriteria());
            sortMode.BindValueChanged(_ => updateCriteria());

            searchTextBox.Current.ValueChanged += _ => updateCriteria();

            updateCriteria();
        }

        public void Deactivate()
        {
            searchTextBox.ReadOnly = true;
            searchTextBox.HoldFocus = false;
            if (searchTextBox.HasFocus)
                GetContainingInputManager().ChangeFocus(searchTextBox);
        }

        public void Activate()
        {
            searchTextBox.ReadOnly = false;
            searchTextBox.HoldFocus = true;
        }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private readonly Bindable<bool> showConverted = new Bindable<bool>();
        private readonly Bindable<double> minimumStars = new BindableDouble();
        private readonly Bindable<double> maximumStars = new BindableDouble();

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnHover(HoverEvent e) => true;

        internal partial class FilterControlTextBox : SeekLimitedSearchTextBox
        {
            private const float filter_text_size = 12;

            public OsuSpriteText FilterText { get; private set; }

            public FilterControlTextBox()
            {
                Height += filter_text_size;
                TextContainer.Height *= (Height - filter_text_size) / Height;
                TextContainer.Margin = new MarginPadding { Bottom = filter_text_size };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                TextContainer.Add(FilterText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Depth = float.MinValue,
                    Font = OsuFont.Default.With(size: filter_text_size, weight: FontWeight.SemiBold),
                    Margin = new MarginPadding { Top = 2, Left = 2 },
                    Colour = colours.Yellow
                });
            }

            public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
            {
                // the "cut" platform key binding (shift-delete) conflicts with the beatmap deletion action.
                if (e.Action == PlatformAction.Cut && e.ShiftPressed && e.CurrentState.Keyboard.Keys.IsPressed(Key.Delete))
                    return false;

                return base.OnPressed(e);
            }
        }
    }
}
