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

        private readonly Box tabStrip;
        private readonly FillFlowContainer<ModeToggleButton> modeButtons;
        private FillFlowContainer resultCounts;

        public readonly SearchTextBox Search;
        public readonly SortTabControl SortTabs;
        public readonly OsuEnumDropdown<RankStatus> RankStatusDropdown;

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
                    Spacing = new Vector2(0f, 10f),
                    Padding = new MarginPadding { Left = DirectOverlay.WIDTH_PADDING, Right = DirectOverlay.WIDTH_PADDING, Top = 10 },
                    Children = new Drawable[]
                    {
                        Search = new DirectSearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        modeButtons = new FillFlowContainer<ModeToggleButton>
                        {
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(10f, 0f),
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
                resultCounts = new FillFlowContainer
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
                        new OsuSpriteText
                        {
                            Text = @"1 Artist, 432 Songs, 3 Tags",
                            TextSize = 15,
                            Font = @"Exo2.0-Bold",
                        },
                    }    
                },
            };

            //todo: possibly restore from config instead of always title
            RankStatusDropdown.Current.Value = RankStatus.RankedApproved;
            SortTabs.Current.Value = SortCriteria.Title;
            SortTabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, RulesetDatabase rulesets, OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
            resultCounts.Colour = colours.Yellow;
            RankStatusDropdown.AccentColour = colours.BlueDark;

            foreach (var r in rulesets.AllRulesets)
            {
                modeButtons.Add(new ModeToggleButton(game?.Ruleset ?? new Bindable<RulesetInfo>(), r));
            }
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

        private class ModeToggleButton : ClickableContainer
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
                icon.FadeTo((Ruleset == obj) ? 1f : 0.5f, 100);
            }

            public ModeToggleButton(Bindable<RulesetInfo> bindable, RulesetInfo ruleset)
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
                Bindable_ValueChanged(null);
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
