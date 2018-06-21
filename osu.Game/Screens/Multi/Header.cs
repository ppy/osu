// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi
{
    public class Header : Container
    {
        public const float HEIGHT = 121;

        private readonly OsuSpriteText screenType;
        private readonly HeaderBreadcrumbControl breadcrumbs;

        public Header(Screen initialScreen)
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"2f2043"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
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
                                new SpriteIcon
                                {
                                    Size = new Vector2(25),
                                    Icon = FontAwesome.fa_osu_multi,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = "multiplayer ",
                                            TextSize = 25,
                                        },
                                        screenType = new OsuSpriteText
                                        {
                                            TextSize = 25,
                                            Font = @"Exo2.0-Light",
                                        },
                                    },
                                },
                            },
                        },
                        breadcrumbs = new HeaderBreadcrumbControl(initialScreen)
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += s => screenType.Text = ((MultiplayerScreen)s).Type.ToLower();
            breadcrumbs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            screenType.Colour = colours.Yellow;
            breadcrumbs.StripColour = colours.Green;
        }

        private class HeaderBreadcrumbControl : ScreenBreadcrumbControl
        {
            public HeaderBreadcrumbControl(Screen initialScreen) : base(initialScreen)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                AccentColour = Color4.White;
            }
        }
    }
}
