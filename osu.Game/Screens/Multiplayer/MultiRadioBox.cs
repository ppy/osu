// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using System;



namespace osu.Game.Screens.Multiplayer
{
    public class MultiRadioBox : Container
    {
        private FlowContainer multiplayerScreen;
        private TabItem activeTab;

        public Action ScreenChanged;
        public MultiRadioBox()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                multiplayerScreen = new FlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FlowDirections.Horizontal,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
            };

            foreach(Screens screen in Enum.GetValues(typeof(Screens)))
            {
                string label = screen.ToString();
                if (label != "Lobby")
                {
                    multiplayerScreen.Add(new TabItem
                    {
                        Text = label,
                        Active = false,
                        Colour = new Color4(255, 255, 255, 179),
                    });
                }
                else
                {
                    multiplayerScreen.Add(activeTab = new TabItem
                    {
                        Text = label,
                        Active = true,
                        Colour = Color4.White,
                    });
                }
            }
        }

        public class TabItem : ClickableContainer
        {
            private SpriteText text;
            private bool active;

            public bool Active
            {
                get { return active; }
                set
                {
                    active = value;
                }
            }
            public string Text
            {
                get { return text.Text; }
                set
                {
                    text.Text = value;
                }
            }

            public void Activate()
            {
                Active = true;
                FadeColour(Color4.White, 300);
            }

            public void Deactivate()
            {
                Active = false;
                FadeColour(new Color4(255, 255, 255, 179), 300);
            }

            public TabItem()
            {
                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 5, Bottom = 5, Left = 10, Right = 10 },
                        TextSize = 20,
                        Font = @"Exo2.0-SemiBold",
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Alpha = 0.7f,
                        Colour = Color4.White,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                    }
                };
            }
            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                if (!active) active = true;
                return base.OnMouseUp(state, args);
            }

            protected override bool OnHover(InputState state)
            {
                if (!active) FadeColour(Color4.White, 300);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                base.OnHoverLost(state);
                if(!active) FadeColour(new Color4(255, 255, 255, 179), 300);
            }
        }
        
        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            foreach (TabItem tab in multiplayerScreen.Children)
            {
                if (tab.Active == true && !(tab.Equals(activeTab)))
                {
                    activeTab.Deactivate();
                    activeTab = tab;
                    activeTab.Activate();

                }
            }
            return base.OnMouseUp(state, args);
        }

        public enum Screens
        {
            Lobby,
            QuickMatch,
            NewGame,
        }
    }
}