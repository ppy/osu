// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Overlays.Direct
{
    public class Header : Container
    {
        public static readonly float HEIGHT = 90;

        private readonly Box tabStrip;

        public readonly OsuTabControl<DirectTab> Tabs;

        public Header()
        {
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"252f3a"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = DirectOverlay.WIDTH_PADDING, Right = DirectOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.BottomLeft,
                            Position = new Vector2(-35f, 5f),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10f, 0f),
                            Children = new Drawable[]
                            {
                                new TextAwesome
                                {
                                    TextSize = 25,
                                    Icon = FontAwesome.fa_osu_chevron_down_o,
                                },
                                new OsuSpriteText
                                {
                                    TextSize = 25,
                                    Text = @"osu!direct",
                                },
                            },
                        },
                        tabStrip = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Width = 282, //todo: make this actually match the tab control's width instead of hardcoding
                            Height = 1,
                        },
                        Tabs = new DirectTabControl
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            Tabs.Current.Value = DirectTab.Search;
            Tabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Green;
        }

        private class DirectTabControl : OsuTabControl<DirectTab>
        {
            protected override TabItem<DirectTab> CreateTabItem(DirectTab value) => new DirectTabItem(value);

            public DirectTabControl()
            {
                Height = 25;
                AccentColour = Color4.White;
            }

            private class DirectTabItem : OsuTabItem
            {
                public DirectTabItem(DirectTab value) : base(value)
                {
                    Text.TextSize = 15;
                }
            }
        }
    }

    public enum DirectTab
    {
        Search,
        [Description("Newest Maps")]
        NewestMaps,
        [Description("Top Rated")]
        TopRated,
        [Description("Most Played")]
        MostPlayed
    }
}
