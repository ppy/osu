// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Direct
{
    public class FilterControl : Container
    {
        /// <summary>
        /// The height of the content below the filter control (tab strip + result count text).
        /// </summary>
        public static readonly float LOWER_HEIGHT = 21;

        private const float padding = 10;

        private readonly Box tabStrip;
        private readonly FillFlowContainer<RulesetToggleButton> modeButtons;
        private FillFlowContainer resultCountsContainer;
        private OsuSpriteText resultCountsText;

        public readonly SearchTextBox Search;
        public readonly SortTabControl SortTabs;
        public readonly OsuEnumDropdown<RankStatus> RankStatusDropdown;

        private ResultCounts resultCounts;
        public  ResultCounts ResultCounts
        {
            get { return resultCounts; }
            set { resultCounts = value; updateResultCounts(); }
        }

        public FilterControl()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"384552"),
                    Alpha = 0.9f,
                },
                tabStrip = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(0f, 1f),
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = DirectOverlay.WIDTH_PADDING, Right = DirectOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        Search = new DirectSearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Top = padding },
                        },
                        modeButtons = new FillFlowContainer<RulesetToggleButton>
                        {
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(padding, 0f),
                            Margin = new MarginPadding { Top = padding },
                        },
                        SortTabs = new SortTabControl
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
                RankStatusDropdown = new SlimEnumDropdown<RankStatus>
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.None,
                    Margin = new MarginPadding { Bottom = 5, Right = DirectOverlay.WIDTH_PADDING },
                    Width = 160f,
                },
                resultCountsContainer = new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Left = DirectOverlay.WIDTH_PADDING, Top = 6 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"Found ",
                            TextSize = 15,
                        },
                        resultCountsText = new OsuSpriteText
                        {
                            TextSize = 15,
                            Font = @"Exo2.0-Bold",
                        },
                    }    
                },
            };

            RankStatusDropdown.Current.Value = RankStatus.RankedApproved;
            SortTabs.Current.Value = SortCriteria.Title;
            SortTabs.Current.TriggerChange();

            updateResultCounts();
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, RulesetDatabase rulesets, OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
            resultCountsContainer.Colour = colours.Yellow;
            RankStatusDropdown.AccentColour = colours.BlueDark;

            foreach (var r in rulesets.AllRulesets)
            {
                modeButtons.Add(new RulesetToggleButton(game?.Ruleset ?? new Bindable<RulesetInfo>(), r));
            }
        }

        private void updateResultCounts()
        {
            resultCountsContainer.FadeTo(ResultCounts == null ? 0 : 1, 200, EasingTypes.Out);
            if (resultCounts == null) return;

            resultCountsText.Text = pluralize(@"Artist", ResultCounts.Artists) + ", " +
                                    pluralize(@"Song", ResultCounts.Songs) + ", " +
                                    pluralize(@"Tag", ResultCounts.Tags);
        }

        private string pluralize(string prefix, int value)
        {
            return $@"{value} {prefix}" + (value == 1 ? @"" : @"s");
        }

        private class DirectSearchTextBox : SearchTextBox
        {
            protected override Color4 BackgroundUnfocused => backgroundColour;
            protected override Color4 BackgroundFocused => backgroundColour;

            private Color4 backgroundColour;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                backgroundColour = colours.Gray2.Opacity(0.9f);
            }
        }

        private class RulesetToggleButton : ClickableContainer
        {
            private TextAwesome icon;

            private RulesetInfo ruleset;
            public RulesetInfo Ruleset
            {
                get { return ruleset; }
                set
                {
                    ruleset = value;
                    icon.Icon = Ruleset.CreateInstance().Icon;
                }
            }

            private Bindable<RulesetInfo> bindable;

            void Bindable_ValueChanged(RulesetInfo obj)
            {
                icon.FadeTo((Ruleset.ID == obj?.ID) ? 1f : 0.5f, 100);
            }

            public RulesetToggleButton(Bindable<RulesetInfo> bindable, RulesetInfo ruleset)
            {
                this.bindable = bindable;
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    icon = new TextAwesome
                    {
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        TextSize = 32,
                    }
                };

                Ruleset = ruleset;
                bindable.ValueChanged += Bindable_ValueChanged;
                Bindable_ValueChanged(bindable.Value);
                Action = () => bindable.Value = Ruleset;
            }

            protected override void Dispose(bool isDisposing)
            {
                if (bindable != null)
                    bindable.ValueChanged -= Bindable_ValueChanged;
                base.Dispose(isDisposing);
            }
        }

        private class SlimEnumDropdown<T> : OsuEnumDropdown<T>
        {
            protected override DropdownHeader CreateHeader() => new SlimDropdownHeader { AccentColour = AccentColour };
            protected override Menu CreateMenu() => new SlimMenu();

            private class SlimDropdownHeader : OsuDropdownHeader
            {
                public SlimDropdownHeader()
                {
                    Height = 25;
                    Icon.TextSize = 16;
                    Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4 };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    BackgroundColour = Color4.Black.Opacity(0.25f);
                }
            }

            private class SlimMenu : OsuMenu
            {
                public SlimMenu()
                {
                    Background.Colour = Color4.Black.Opacity(0.25f);
                }
            }
        }
    }

    public class ResultCounts
    {
        public readonly int Artists;
        public readonly int Songs;
        public readonly int Tags;
        
        public ResultCounts(int artists, int songs, int tags)
        {
            Artists = artists;
            Songs = songs;
            Tags = tags;
        }
    }

    public enum RankStatus
    {
        Any,
        [Description("Ranked & Approved")]
        RankedApproved,
        Approved,
        Loved,
        Favourites,
        [Description("Mod Requests")]
        ModRequests,
        Pending,
        Graveyard,
        [Description("My Maps")]
        MyMaps,
    }
}
