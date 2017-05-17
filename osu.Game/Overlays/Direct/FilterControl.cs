// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Direct
{
    public class FilterControl : Container
    {
        private readonly Box tabStrip;
        private readonly FillFlowContainer<ModeToggleButton> modeButtons;
        private readonly OsuDropdown<RankStatus> rankStatusDropdown;

        public readonly SearchTextBox Search;

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

        protected override bool InternalContains(Vector2 screenSpacePos) => true;

        public FilterControl()
        {
            RelativeSizeAxes = Axes.X;
            //AutoSizeAxes = Axes.Y;
            Height = 127;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"384552"),
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
                    Padding = new MarginPadding { Left = DirectOverlay.WIDTH_PADDING, Right = DirectOverlay.WIDTH_PADDING, Top = 10 },
                    Spacing = new Vector2(0f, 10f),
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
                        new SortTabControl
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
                rankStatusDropdown = new SlimEnumDropdown<RankStatus>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Margin = new MarginPadding { Top = 93, Bottom = 5, Right = DirectOverlay.WIDTH_PADDING }, //todo: sort of hacky positioning
                    Width = 160f,
                },
            };

            rankStatusDropdown.Current.Value = RankStatus.RankedApproved;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, RulesetDatabase rulesets, OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
            rankStatusDropdown.AccentColour = colours.BlueDark;

            foreach (var r in rulesets.AllRulesets)
            {
                modeButtons.Add(new ModeToggleButton(game?.Ruleset ?? new Bindable<RulesetInfo>(), r));
            }
        }

        private class DirectSearchTextBox : SearchTextBox
        {
            protected override Color4 BackgroundUnfocused => OsuColour.FromHex(@"222222");
            protected override Color4 BackgroundFocused => OsuColour.FromHex(@"222222");
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

            private class SlimDropdownHeader : OsuDropdownHeader
            {
                public SlimDropdownHeader()
                {
                    Height = 25;
                    Icon.TextSize = 16;
                    Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 8, Right = 4, };
                }
            }
        }
    }
}
