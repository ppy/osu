// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Filter;
using Container = osu.Framework.Graphics.Containers.Container;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select
{
    public class FilterControl : Container
    {
        public const float HEIGHT = 100;

        public Action<FilterCriteria> FilterChanged;

        private readonly OsuTabControl<SortMode> sortTabs;

        private readonly TabControl<GroupMode> groupTabs;

        public readonly Bindable<SortMode> SortMode = new Bindable<SortMode>(Filter.SortMode.Title);

        public readonly Bindable<GroupMode> GroupMode = new Bindable<GroupMode>(Filter.GroupMode.All);

        public FilterCriteria CreateCriteria() => new FilterCriteria
        {
            Group = GroupMode.Value,
            Sort = SortMode.Value,
            SearchText = searchTextBox.Text,
            AllowConvertedBeatmaps = showConverted.Value,
            Ruleset = ruleset.Value
        };

        public Action Exit;

        private readonly SearchTextBox searchTextBox;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            base.ReceivePositionalInputAt(screenSpacePos) || groupTabs.ReceivePositionalInputAt(screenSpacePos) || sortTabs.ReceivePositionalInputAt(screenSpacePos);

        public FilterControl()
        {
            Children = new Drawable[]
            {
                Background = new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
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
                        searchTextBox = new SearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Exit = () => Exit?.Invoke(),
                        },
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
                            Children = new Drawable[]
                            {
                                groupTabs = new OsuTabControl<GroupMode>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 24,
                                    Width = 0.5f,
                                    AutoSort = true,
                                    Current = GroupMode
                                },
                                //spriteText = new OsuSpriteText
                                //{
                                //    Font = @"Exo2.0-Bold",
                                //    Text = "Sort results by",
                                //    Size = 14,
                                //    Margin = new MarginPadding
                                //    {
                                //        Top = 5,
                                //        Bottom = 5
                                //    },
                                //},
                                sortTabs = new OsuTabControl<SortMode>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.5f,
                                    Height = 24,
                                    AutoSort = true,
                                    Current = SortMode
                                }
                            }
                        },
                    }
                }
            };

            searchTextBox.Current.ValueChanged += _ => FilterChanged?.Invoke(CreateCriteria());

            groupTabs.PinItem(Filter.GroupMode.All);
            groupTabs.PinItem(Filter.GroupMode.RecentlyPlayed);
        }

        public void Deactivate()
        {
            searchTextBox.HoldFocus = false;
            if (searchTextBox.HasFocus)
                GetContainingInputManager().ChangeFocus(searchTextBox);
        }

        public void Activate()
        {
            searchTextBox.HoldFocus = true;
        }

        private readonly IBindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private Bindable<bool> showConverted;

        public readonly Box Background;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, IBindable<RulesetInfo> parentRuleset, OsuConfigManager config)
        {
            sortTabs.AccentColour = colours.GreenLight;

            showConverted = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps);
            showConverted.ValueChanged += _ => updateCriteria();

            ruleset.BindTo(parentRuleset);
            ruleset.BindValueChanged(_ => updateCriteria());

            config.BindWith(OsuSetting.SelectGroupingMode, GroupMode);
            config.BindWith(OsuSetting.SelectSortingMode, SortMode);

            GroupMode.BindValueChanged(_ => updateCriteria());
            SortMode.BindValueChanged(_ => updateCriteria());

            updateCriteria();
        }

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());
    }
}
