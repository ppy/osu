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

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public class Header : Container
    {
        public const float HEIGHT = 50;

        private readonly OsuSpriteText screenType;
        private readonly HeaderBreadcrumbControl breadcrumbs;
        private readonly Container boxContainer;
        private readonly FillFlowContainer textContainer;
        private readonly FillFlowContainer childModeButtons;

        public Header(Screen initialScreen)
        {
            Children = new Drawable[]
            {
                boxContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    //CornerRadius = 20,
                    //Masking = true,
                    Margin = new MarginPadding { Left = 50, Top = 20 },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        // This does not create a visible box above the triangles to indicate the end of the area of each screen
                        new Container
                        {
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = 50, Top = 20 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Height = 50,
                            //Size = new Vector2(1),
                            Colour = OsuColour.FromHex("1a2328"),
                        },
                        textContainer = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(7.5f, 0f),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Size = new Vector2(40),
                                    Icon = FontAwesome.fa_osu_edit_o,
                                },
                                new OsuSpriteText
                                {
                                    Margin = new MarginPadding { Top = 7.5f },
                                    Text = "Beatmap Setup",
                                    TextSize = 25,
                                },
                                screenType = new OsuSpriteText
                                {
                                    TextSize = 25,
                                    Margin = new MarginPadding { Top = 7.5f },
                                    Text = "General",
                                    Font = @"Exo2.0-Light",
                                },
                            }
                        },
                    },
                },
            };
            //RelativeSizeAxes = Axes.X;
            //Height = HEIGHT;
            //Anchor = Anchor.Centre;
            //CornerRadius = 20;
            //Size = new Vector2(0.6f);
            //Children = new Drawable[]
            //{
            //    new Box
            //    {
            //        RelativeSizeAxes = Axes.Both,
            //        Colour = OsuColour.FromHex(@"192428"),
            //    },
            //    new Container
            //    {
            //        RelativeSizeAxes = Axes.Both,
            //        Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
            //        Children = new Drawable[]
            //        {
            //            new FillFlowContainer
            //            {
            //                Anchor = Anchor.CentreLeft,
            //                Origin = Anchor.BottomLeft,
            //                //Position = new Vector2(-35f, 5f),
            //                AutoSizeAxes = Axes.Both,
            //                Direction = FillDirection.Horizontal,
            //                Spacing = new Vector2(10f, 0f),
            //                Children = new Drawable[]
            //                {
            //                    new SpriteIcon
            //                    {
            //                        Size = new Vector2(25),
            //                        Icon = FontAwesome.fa_osu_edit_o,
            //                    },
            //                    new FillFlowContainer
            //                    {
            //                        AutoSizeAxes = Axes.Both,
            //                        Direction = FillDirection.Horizontal,
            //                        Children = new[]
            //                        {
            //                            new OsuSpriteText
            //                            {
            //                                Text = "Beatmap Setup ",
            //                                TextSize = 25,
            //                            },
            //                            screenType = new OsuSpriteText
            //                            {
            //                                TextSize = 25,
            //                                Font = @"Exo2.0-Light",
            //                            },
            //                        },
            //                    },
            //                },
            //            },
            //        },
            //    },
            //};
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            screenType.Colour = colours.BlueDark;
        }

        public void UpdateScreen(string screenName) => screenType.Text = screenName;

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
