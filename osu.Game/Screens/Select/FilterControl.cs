// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public class FilterControl : Container
    {
        public const float HEIGHT = 2 * side_margin + 85;
        private const float side_margin = 20;

        public Action<FilterCriteria> FilterChanged;

        private OsuTabControl<SortMode> sortTabs;

        private Bindable<SortMode> sortMode;

        private Bindable<GroupMode> groupMode;

        public FilterCriteria CreateCriteria()
        {
            var query = searchTextBox.Text;

            var criteria = new FilterCriteria
            {
                Group = groupMode.Value,
                Sort = sortMode.Value,
                AllowConvertedBeatmaps = showConverted.Value,
                Ruleset = ruleset.Value,
                Collection = collectionDropdown?.Current.Value
            };

            if (!minimumStars.IsDefault)
                criteria.UserStarDifficulty.Min = minimumStars.Value;

            if (!maximumStars.IsDefault)
                criteria.UserStarDifficulty.Max = maximumStars.Value;

            FilterQueryParser.ApplyQueries(criteria, query);
            return criteria;
        }

        private SeekLimitedSearchTextBox searchTextBox;
        private CollectionFilterDropdown collectionDropdown;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            base.ReceivePositionalInputAt(screenSpacePos) || sortTabs.ReceivePositionalInputAt(screenSpacePos);

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, IBindable<RulesetInfo> parentRuleset, OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ShowConvertedBeatmaps, showConverted);
            showConverted.ValueChanged += _ => updateCriteria();

            config.BindWith(OsuSetting.DisplayStarsMinimum, minimumStars);
            minimumStars.ValueChanged += _ => updateCriteria();

            config.BindWith(OsuSetting.DisplayStarsMaximum, maximumStars);
            maximumStars.ValueChanged += _ => updateCriteria();

            ruleset.BindTo(parentRuleset);
            ruleset.BindValueChanged(_ => updateCriteria());

            sortMode = config.GetBindable<SortMode>(OsuSetting.SongSelectSortingMode);
            groupMode = config.GetBindable<GroupMode>(OsuSetting.SongSelectGroupingMode);

            groupMode.BindValueChanged(_ => updateCriteria());
            sortMode.BindValueChanged(_ => updateCriteria());

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
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 60),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension(GridSizeMode.Absolute, 20),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        searchTextBox = new SeekLimitedSearchTextBox { RelativeSizeAxes = Axes.X },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 1,
                                            Colour = OsuColour.Gray(80),
                                            Origin = Anchor.BottomLeft,
                                            Anchor = Anchor.BottomLeft,
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            Direction = FillDirection.Horizontal,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Spacing = new Vector2(OsuTabControl<SortMode>.HORIZONTAL_SPACING, 0),
                                            Children = new Drawable[]
                                            {
                                                new OsuTabControlCheckbox
                                                {
                                                    Text = "Show converted",
                                                    Current = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                },
                                                sortTabs = new OsuTabControl<SortMode>
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Width = 0.5f,
                                                    Height = 24,
                                                    AutoSort = true,
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                    AccentColour = colours.GreenLight,
                                                    Current = { BindTarget = sortMode }
                                                },
                                                new OsuSpriteText
                                                {
                                                    Text = "Sort by",
                                                    Font = OsuFont.GetFont(size: 14),
                                                    Margin = new MarginPadding(5),
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                },
                                            }
                                        },
                                    }
                                }
                            },
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        collectionDropdown = new CollectionFilterDropdown
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.4f,
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };

            collectionDropdown.Current.ValueChanged += _ => updateCriteria();
            searchTextBox.Current.ValueChanged += _ => updateCriteria();

            updateCriteria();
        }

        private class CollectionFilterDropdown : OsuDropdown<CollectionFilter>
        {
            private readonly IBindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();
            private readonly IBindableList<BeatmapInfo> beatmaps = new BindableList<BeatmapInfo>();
            private readonly BindableList<CollectionFilter> filters = new BindableList<CollectionFilter>();

            public CollectionFilterDropdown()
            {
                ItemSource = filters;
            }

            [BackgroundDependencyLoader]
            private void load(BeatmapCollectionManager collectionManager)
            {
                collections.BindTo(collectionManager.Collections);
                collections.CollectionChanged += (_, __) => collectionsChanged();
                collectionsChanged();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(filterChanged, true);
            }

            /// <summary>
            /// Occurs when a collection has been added or removed.
            /// </summary>
            private void collectionsChanged()
            {
                var selectedItem = SelectedItem?.Value?.Collection;

                filters.Clear();
                filters.Add(new CollectionFilter(null));
                filters.AddRange(collections.Select(c => new CollectionFilter(c)));

                Current.Value = filters.SingleOrDefault(f => f.Collection == selectedItem) ?? filters[0];
            }

            /// <summary>
            /// Occurs when the <see cref="CollectionFilter"/> selection has changed.
            /// </summary>
            private void filterChanged(ValueChangedEvent<CollectionFilter> filter)
            {
                beatmaps.CollectionChanged -= filterBeatmapsChanged;

                if (filter.OldValue?.Collection != null)
                    beatmaps.UnbindFrom(filter.OldValue.Collection.Beatmaps);

                if (filter.NewValue?.Collection != null)
                    beatmaps.BindTo(filter.NewValue.Collection.Beatmaps);

                beatmaps.CollectionChanged += filterBeatmapsChanged;
            }

            /// <summary>
            /// Occurs when the beatmaps contained by a <see cref="BeatmapCollection"/> have changed.
            /// </summary>
            private void filterBeatmapsChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                // The filtered beatmaps have changed, without the filter having changed itself. So a change in filter must be notified.
                // Note that this does NOT propagate to bound bindables, so the FilterControl must bind directly to the value change event of this bindable.
                Current.TriggerChange();
            }

            protected override string GenerateItemText(CollectionFilter item) => item.Collection?.Name ?? "All beatmaps";

            protected override DropdownHeader CreateHeader() => new CollectionDropdownHeader();

            protected override DropdownMenu CreateMenu() => new CollectionDropdownMenu();

            private class CollectionDropdownHeader : OsuDropdownHeader
            {
                public CollectionDropdownHeader()
                {
                    Height = 25;
                    Icon.Size = new Vector2(16);
                    Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
                }
            }

            private class CollectionDropdownMenu : OsuDropdownMenu
            {
                public CollectionDropdownMenu()
                {
                    MaxHeight = 200;
                }

                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new CollectionDropdownMenuItem(item);
            }

            private class CollectionDropdownMenuItem : OsuDropdownMenu.DrawableOsuDropdownMenuItem
            {
                [Resolved]
                private OsuColour colours { get; set; }

                [Resolved]
                private IBindable<WorkingBeatmap> beatmap { get; set; }

                [CanBeNull]
                private readonly BindableList<BeatmapInfo> collectionBeatmaps;

                private IconButton addOrRemoveButton;

                public CollectionDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                    collectionBeatmaps = ((DropdownMenuItem<CollectionFilter>)item).Value.Collection?.Beatmaps.GetBoundCopy();
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AddRangeInternal(new Drawable[]
                    {
                        addOrRemoveButton = new IconButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            X = -OsuScrollContainer.SCROLL_BAR_HEIGHT,
                            Scale = new Vector2(0.75f),
                            Alpha = collectionBeatmaps == null ? 0 : 1,
                            Action = addOrRemove
                        }
                    });
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    if (collectionBeatmaps != null)
                    {
                        collectionBeatmaps.CollectionChanged += (_, __) => collectionChanged();
                        beatmap.BindValueChanged(_ => collectionChanged(), true);
                    }
                }

                private void collectionChanged()
                {
                    Debug.Assert(collectionBeatmaps != null);

                    addOrRemoveButton.Enabled.Value = !beatmap.IsDefault;

                    if (collectionBeatmaps.Contains(beatmap.Value.BeatmapInfo))
                    {
                        addOrRemoveButton.Icon = FontAwesome.Solid.MinusSquare;
                        addOrRemoveButton.IconColour = colours.Red;
                    }
                    else
                    {
                        addOrRemoveButton.Icon = FontAwesome.Solid.PlusSquare;
                        addOrRemoveButton.IconColour = colours.Green;
                    }
                }

                private void addOrRemove()
                {
                    Debug.Assert(collectionBeatmaps != null);

                    if (!collectionBeatmaps.Remove(beatmap.Value.BeatmapInfo))
                        collectionBeatmaps.Add(beatmap.Value.BeatmapInfo);
                }
            }
        }

        public class CollectionFilter
        {
            [CanBeNull]
            public readonly BeatmapCollection Collection;

            public CollectionFilter([CanBeNull] BeatmapCollection collection)
            {
                Collection = collection;
            }

            public virtual bool ContainsBeatmap(BeatmapInfo beatmap)
                => Collection?.Beatmaps.Any(b => b.Equals(beatmap)) ?? true;
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

        private readonly IBindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<bool> showConverted = new Bindable<bool>();
        private readonly Bindable<double> minimumStars = new BindableDouble();
        private readonly Bindable<double> maximumStars = new BindableDouble();

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnHover(HoverEvent e) => true;
    }
}
