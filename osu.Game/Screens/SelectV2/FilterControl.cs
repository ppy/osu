// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    public partial class FilterControl : OverlayContainer
    {
        // taken from draw visualiser. used for carousel alignment purposes.
        public const float HEIGHT_FROM_SCREEN_TOP = 141 - corner_radius;

        private const float corner_radius = 8;

        private SongSelectSearchTextBox searchTextBox = null!;
        private ShearedToggleButton showConvertedBeatmapsButton = null!;
        private DifficultyRangeSlider difficultyRangeSlider = null!;
        private ShearedDropdown<SortMode> sortDropdown = null!;
        private ShearedDropdown<GroupMode> groupDropdown = null!;
        private CollectionDropdown collectionDropdown = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        public LocalisableString StatusText
        {
            get => searchTextBox.StatusText;
            set => searchTextBox.StatusText = value;
        }

        public event Action<FilterCriteria>? CriteriaChanged;

        private FilterCriteria currentCriteria = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Shear = OsuGame.SHEAR;
            Margin = new MarginPadding { Top = -corner_radius, Right = -40 };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = corner_radius,
                    Masking = true,
                    Child = new WedgeBackground
                    {
                        Anchor = Anchor.TopRight,
                        Scale = new Vector2(-1, 1),
                    }
                },
                new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 5f),
                    Padding = new MarginPadding { Top = corner_radius + 5, Bottom = 2, Right = 40f, Left = 2f },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            Child = searchTextBox = new SongSelectSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                HoldFocus = true,
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute), // can probably be removed?
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    difficultyRangeSlider = new DifficultyRangeSlider
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        MinRange = 0.1f,
                                    },
                                    Empty(),
                                    showConvertedBeatmapsButton = new ShearedToggleButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = UserInterfaceStrings.ShowConverts,
                                        Height = 30f,
                                    },
                                },
                            }
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Shear = -OsuGame.SHEAR,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(maxSize: 210),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(maxSize: 230),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    sortDropdown = new ShearedDropdown<SortMode>(SortStrings.Default)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<SortMode>(),
                                    },
                                    Empty(),
                                    // todo: pending localisation
                                    groupDropdown = new ShearedDropdown<GroupMode>("Group by")
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Items = Enum.GetValues<GroupMode>(),
                                    },
                                    Empty(),
                                    collectionDropdown = new CollectionDropdown
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                }
                            }
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            difficultyRangeSlider.LowerBound = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            difficultyRangeSlider.UpperBound = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);
            config.BindWith(OsuSetting.ShowConvertedBeatmaps, showConvertedBeatmapsButton.Active);
            config.BindWith(OsuSetting.SongSelectSortingMode, sortDropdown.Current);
            config.BindWith(OsuSetting.SongSelectGroupMode, groupDropdown.Current);

            ruleset.BindValueChanged(_ => updateCriteria());
            mods.BindValueChanged(m =>
            {
                // The following is a note carried from old song select and may not be a valid reason anymore:
                // // Mods are updated once by the mod select overlay when song select is entered,
                // // regardless of if there are any mods or any changes have taken place.
                // // Updating the criteria here so early triggers a re-ordering of panels on song select, via... some mechanism.
                // // Todo: Investigate/fix and potentially remove this.
                // TODO: this might be simply removable with the new song select & carousel code.
                if (m.NewValue.SequenceEqual(m.OldValue))
                    return;

                var rulesetCriteria = currentCriteria.RulesetCriteria;
                if (rulesetCriteria?.FilterMayChangeFromMods(m) == true)
                    updateCriteria();
            });

            searchTextBox.Current.BindValueChanged(_ => updateCriteria());
            difficultyRangeSlider.LowerBound.BindValueChanged(_ => updateCriteria());
            difficultyRangeSlider.UpperBound.BindValueChanged(_ => updateCriteria());
            showConvertedBeatmapsButton.Active.BindValueChanged(_ => updateCriteria());
            sortDropdown.Current.BindValueChanged(_ => updateCriteria());
            groupDropdown.Current.BindValueChanged(_ => updateCriteria());
            collectionDropdown.Current.BindValueChanged(_ => updateCriteria());
            updateCriteria();
        }

        /// <summary>
        /// Creates a <see cref="FilterCriteria"/> based on the current state of the controls.
        /// </summary>
        public FilterCriteria CreateCriteria()
        {
            string query = searchTextBox.Current.Value;

            var criteria = new FilterCriteria
            {
                Sort = sortDropdown.Current.Value,
                Group = groupDropdown.Current.Value,
                AllowConvertedBeatmaps = showConvertedBeatmapsButton.Active.Value,
                Ruleset = ruleset.Value,
                Mods = mods.Value,
                CollectionBeatmapMD5Hashes = collectionDropdown.Current.Value?.Collection?.PerformRead(c => c.BeatmapMD5Hashes).ToImmutableHashSet()
            };

            if (!difficultyRangeSlider.LowerBound.IsDefault)
                criteria.UserStarDifficulty.Min = difficultyRangeSlider.LowerBound.Value;

            if (!difficultyRangeSlider.UpperBound.IsDefault)
                criteria.UserStarDifficulty.Max = difficultyRangeSlider.UpperBound.Value;

            criteria.RulesetCriteria = ruleset.Value.CreateInstance().CreateRulesetFilterCriteria();

            FilterQueryParser.ApplyQueries(criteria, query);
            return criteria;
        }

        private void updateCriteria()
        {
            currentCriteria = CreateCriteria();
            CriteriaChanged?.Invoke(currentCriteria);
        }

        /// <summary>
        /// Set the query to the search text box.
        /// </summary>
        /// <param name="query">The string to search.</param>
        public void Search(string query)
        {
            searchTextBox.Current.Value = query;
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        private partial class SongSelectSearchTextBox : ShearedFilterTextBox
        {
            protected override InnerSearchTextBox CreateInnerTextBox() => new InnerTextBox();

            private partial class InnerTextBox : InnerFilterTextBox
            {
                public override bool HandleLeftRightArrows => false;

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
}
