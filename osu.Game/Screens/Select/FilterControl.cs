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
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select
{
    public class FilterControl : Container
    {
        public const float HEIGHT = 100;

        public Action<FilterCriteria> FilterChanged;

        private readonly OsuTabControl<SortMode> sortTabs;

        private readonly TabControl<GroupMode> groupTabs;

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
                Ruleset = ruleset.Value
            };

            applyQueries(criteria, ref query);

            criteria.SearchText = query;

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
                                    AutoSort = true,
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

            searchTextBox.Current.ValueChanged += _ => FilterChanged?.Invoke(CreateCriteria());

            groupTabs.PinItem(GroupMode.All);
            groupTabs.PinItem(GroupMode.RecentlyPlayed);
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

            sortMode = config.GetBindable<SortMode>(OsuSetting.SongSelectSortingMode);
            groupMode = config.GetBindable<GroupMode>(OsuSetting.SongSelectGroupingMode);

            sortTabs.Current.BindTo(sortMode);
            groupTabs.Current.BindTo(groupMode);

            groupMode.BindValueChanged(_ => updateCriteria());
            sortMode.BindValueChanged(_ => updateCriteria());

            updateCriteria();
        }

        private void updateCriteria() => FilterChanged?.Invoke(CreateCriteria());

        private static readonly Regex query_syntax_regex = new Regex(
            @"\b(?<key>stars|ar|dr|cs|divisor|length|objects|bpm|status)(?<op>[=:><]+)(?<value>\S*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void applyQueries(FilterCriteria criteria, ref string query)
        {
            foreach (Match match in query_syntax_regex.Matches(query))
            {
                var key = match.Groups["key"].Value.ToLower();
                var op = match.Groups["op"].Value;
                var value = match.Groups["value"].Value;

                switch (key)
                {
                    case "stars" when float.TryParse(value, out var stars):
                        updateCriteriaRange(ref criteria.StarDifficulty, op, stars);
                        break;

                    case "ar" when float.TryParse(value, out var ar):
                        updateCriteriaRange(ref criteria.ApproachRate, op, ar);
                        break;

                    case "dr" when float.TryParse(value, out var dr):
                        updateCriteriaRange(ref criteria.DrainRate, op, dr);
                        break;

                    case "cs" when float.TryParse(value, out var cs):
                        updateCriteriaRange(ref criteria.CircleSize, op, cs);
                        break;

                    case "bpm" when double.TryParse(value, out var bpm):
                        updateCriteriaRange(ref criteria.BPM, op, bpm);
                        break;

                    case "length" when double.TryParse(value.TrimEnd('m', 's', 'h'), out var length):
                        var scale =
                            value.EndsWith("ms") ? 1 :
                            value.EndsWith("s") ? 1000 :
                            value.EndsWith("m") ? 60000 :
                            value.EndsWith("h") ? 3600000 : 1000;

                        updateCriteriaRange(ref criteria.Length, op, length * scale, scale / 2.0);
                        break;

                    case "divisor" when int.TryParse(value, out var divisor):
                        updateCriteriaRange(ref criteria.BeatDivisor, op, divisor);
                        break;

                    case "status" when Enum.TryParse<BeatmapSetOnlineStatus>(value, true, out var statusValue):
                        updateCriteriaRange(ref criteria.OnlineStatus, op, statusValue);
                        break;
                }

                query = query.Replace(match.ToString(), "");
            }
        }

        private void updateCriteriaRange(ref FilterCriteria.OptionalRange<float> range, string op, float value, float tolerance = 0.05f)
        {
            updateCriteriaRange(ref range, op, value);

            switch (op)
            {
                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;
            }
        }

        private void updateCriteriaRange(ref FilterCriteria.OptionalRange<double> range, string op, double value, double tolerance = 0.05)
        {
            updateCriteriaRange(ref range, op, value);

            switch (op)
            {
                case "=":
                case ":":
                    range.Min = value - tolerance;
                    range.Max = value + tolerance;
                    break;
            }
        }

        private void updateCriteriaRange<T>(ref FilterCriteria.OptionalRange<T> range, string op, T value)
            where T : struct, IComparable
        {
            switch (op)
            {
                default:
                    return;

                case "=":
                case ":":
                    range.IsInclusive = true;
                    range.Min = value;
                    range.Max = value;
                    break;

                case ">":
                    range.IsInclusive = false;
                    range.Min = value;
                    break;

                case ">=":
                case ">:":
                    range.IsInclusive = true;
                    range.Min = value;
                    break;

                case "<":
                    range.IsInclusive = false;
                    range.Max = value;
                    break;

                case "<=":
                case "<:":
                    range.IsInclusive = true;
                    range.Max = value;
                    break;
            }
        }
    }
}
