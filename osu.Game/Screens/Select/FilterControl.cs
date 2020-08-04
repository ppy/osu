// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
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
        public const float HEIGHT = 100;

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
            };

            if (!minimumStars.IsDefault)
                criteria.UserStarDifficulty.Min = minimumStars.Value;

            if (!maximumStars.IsDefault)
                criteria.UserStarDifficulty.Max = maximumStars.Value;

            FilterQueryParser.ApplyQueries(criteria, query);
            return criteria;
        }

        private SeekLimitedSearchTextBox searchTextBox;

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
                    Padding = new MarginPadding(20),
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
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
            };

            searchTextBox.Current.ValueChanged += _ => FilterChanged?.Invoke(CreateCriteria());

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

        private readonly IBindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly Bindable<bool> showConverted = new Bindable<bool>();
        private readonly Bindable<double> minimumStars = new BindableDouble();
        private readonly Bindable<double> maximumStars = new BindableDouble();

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnHover(HoverEvent e) => true;
    }
}
