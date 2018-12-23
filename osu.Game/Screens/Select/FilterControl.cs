// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osuTK;
using osuTK.Graphics;
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
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using System.Text.RegularExpressions;

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

        static readonly Regex query_syntax_regex = new Regex(@"\b(?<key>stars|ar|divisor|length)(?<op>:|>|<)(?<value>\w+)\b");

        enum QueryOperation
        {
            Equals,
            LargerThan,
            LessThan
        }

        public FilterCriteria CreateCriteria()
        {
            var text = searchTextBox.Text;
            var criteria = new FilterCriteria
            {
                Group = group,
                Sort = sort,
                SearchText = query_syntax_regex.Replace(text, string.Empty),
                AllowConvertedBeatmaps = showConverted,
                Ruleset = ruleset.Value
            };

            foreach (Match match in query_syntax_regex.Matches(text))
            {
                QueryOperation op;

                switch (match.Groups["op"].Value)
                {
                    default:
                    case ":": op = QueryOperation.Equals; break;
                    case ">": op = QueryOperation.LargerThan; break;
                    case "<": op = QueryOperation.LessThan; break;
                }

                switch (match.Groups["key"].Value)
                {
                    case "stars":
                        var stars = Convert.ToDouble(match.Groups["value"].Value);
                        switch (op)
                        {
                            case QueryOperation.Equals: criteria.StarDifficulty.Min = stars; criteria.StarDifficulty.Max = stars + 1; break;
                            case QueryOperation.LargerThan: criteria.StarDifficulty.Min = stars; break;
                            case QueryOperation.LessThan: criteria.StarDifficulty.Max = stars; break;
                        }
                        break;
                    case "ar":
                        var ar = Convert.ToDouble(match.Groups["value"].Value);
                        switch (op)
                        {
                            case QueryOperation.Equals: criteria.ApproachRate.Min = ar; criteria.ApproachRate.Max = ar + 1; break;
                            case QueryOperation.LargerThan: criteria.ApproachRate.Min = ar; break;
                            case QueryOperation.LessThan: criteria.ApproachRate.Max = ar; break;
                        }
                        break;
                    case "divisor":
                        var divisor = Convert.ToDouble(match.Groups["value"].Value);
                        switch (op)
                        {
                            case QueryOperation.Equals: criteria.BeatDivisor = unchecked((int)divisor); break;
                        }
                        break;
                    case "length":
                        var lengthStr = match.Groups["value"].Value;
                        // Length is measured in milliseconds
                        var length =
                            lengthStr.EndsWith("ms") ? Convert.ToDouble(lengthStr.TrimEnd('m', 's')) :
                            lengthStr.EndsWith("s") ? Convert.ToDouble(lengthStr.TrimEnd('s')) * 1000 :
                            lengthStr.EndsWith("m") ? Convert.ToDouble(lengthStr.TrimEnd('m')) * 60000 :
                            lengthStr.EndsWith("h") ? Convert.ToDouble(lengthStr.TrimEnd('h')) * 3600000 :
                            Convert.ToDouble(lengthStr) * 1000;

                        switch (op)
                        {
                            case QueryOperation.Equals: criteria.Length.Min = length; criteria.Length.Max = length + 60000; break;
                            case QueryOperation.LargerThan: criteria.Length.Min = length; break;
                            case QueryOperation.LessThan: criteria.Length.Max = length; break;
                        }
                        break;
                }
            }

            return criteria;
        }

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

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;
    }
}
