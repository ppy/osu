// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multiplayer
{
    public class Header : Container
    {
        private readonly OsuSpriteText screenTitle;
        private readonly Box tabStrip;
        private readonly BreadcrumbControl<MultiplayerScreen> breadcrumbs;

        private MultiplayerScreen currentScreen;
        public MultiplayerScreen CurrentScreen
        {
            get { return currentScreen; }
            set
            {
                if (value == currentScreen) return;

                if (CurrentScreen != null)
                {
                    CurrentScreen.Exited -= onExited;
                    CurrentScreen.ModePushed -= onPushed;
                }
                else
                {
                    // this is the first screen in the stack, so call the initial onPushed
                    currentScreen = value;
                    onPushed(CurrentScreen);
                }

                currentScreen = value;

                if (CurrentScreen != null)
                {
                    CurrentScreen.Exited += onExited;
                    CurrentScreen.ModePushed += onPushed;
                    breadcrumbs.Current.Value = CurrentScreen;
                }
            }
        }

        public Header()
        {
            RelativeSizeAxes = Axes.X;
            Height = 121;

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
                                        screenTitle = new OsuSpriteText
                                        {
                                            TextSize = 25,
                                            Font = @"Exo2.0-Light",
                                        },
                                    },
                                },
                            },
                        },
                        tabStrip = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Width = 0,
                            Height = 1,
                        },
                        breadcrumbs = new BreadcrumbControl<MultiplayerScreen>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            OnLoadComplete = d => breadcrumbs.AccentColour = Color4.White,
                        },
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += s =>
            {
                if (s != CurrentScreen)
                {
                    CurrentScreen = s;
                    s.MakeCurrent();
                }

                screenTitle.Text = s.Title;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            screenTitle.Colour = colours.Yellow;
            tabStrip.Colour = colours.Green;
        }

        private void onExited(Screen screen)
        {
            CurrentScreen = screen as MultiplayerScreen;
        }

        private void onPushed(Screen screen)
        {
            var newScreen = screen as MultiplayerScreen;

            breadcrumbs.Items.ToList().SkipWhile(i => i != breadcrumbs.Current.Value).Skip(1).ForEach(i => breadcrumbs.RemoveItem(i));
            breadcrumbs.AddItem(newScreen);

            CurrentScreen = newScreen;
        }
    }
}
