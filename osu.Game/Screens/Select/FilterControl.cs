// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Filter;
using Container = osu.Framework.Graphics.Containers.Container;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Configuration;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select
{
    public class FilterControl : Container
    {
        public Action<FilterCriteria> FilterChanged;

        private readonly OsuTabControl<SortMode> sortTabs;

        private readonly TabControl<GroupMode> groupTabs;

        private SortMode sort = SortMode.Title;

        public SortMode Sort
        {
            get { return sort; }
            set
            {
                if (sort != value)
                {
                    sort = value;
                    FilterChanged?.Invoke(CreateCriteria());
                }
            }
        }

        private GroupMode group = GroupMode.All;

        public GroupMode Group
        {
            get { return group; }
            set
            {
                if (group != value)
                {
                    group = value;
                    FilterChanged?.Invoke(CreateCriteria());
                }
            }
        }

        public FilterCriteria CreateCriteria() => new FilterCriteria
        {
            Group = group,
            Sort = sort,
            SearchText = searchTextBox.Text,
            AllowConvertedBeatmaps = showConverted,
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
                                    AutoSort = true
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
                                }
                            }
                        },
                    }
                }
            };

            searchTextBox.Current.ValueChanged += t => FilterChanged?.Invoke(CreateCriteria());

            groupTabs.PinItem(GroupMode.All);
            groupTabs.PinItem(GroupMode.RecentlyPlayed);
            groupTabs.Current.ValueChanged += val => Group = val;
            sortTabs.Current.ValueChanged += val => Sort = val;
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
            showConverted.ValueChanged += val => updateCriteria();

            ruleset.BindTo(parentRuleset);
            ruleset.BindValueChanged(_ => updateCriteria(), true);
        }

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnMouseMove(InputState state) => true;

        protected override bool OnClick(InputState state) => true;
    }
}
