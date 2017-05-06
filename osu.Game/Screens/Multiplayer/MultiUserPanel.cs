// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiUserPanel : ClickableContainer, IStateful<MultiUserPanel.UserState>
    {
        private UserState state;
        public UserState State
        {
            get { return state; }
            set
            {
                if (state == value)
                    return;

                state = value;
                switch (state)
                {
                    case UserState.Guest:
                        statusString = "Hosting a Room";
                        UpdatePanel(this);
                        break;

                    case UserState.Host:
                        statusString = "Waiting in a Room";
                        UpdatePanel(this);
                        break;
                }
            }
        }

        private string userName;
        private string statusString;
        private OsuSpriteText userSprite;
        private OsuSpriteText statusSprite;

        public const int PANEL_HEIGHT = 100;
        public const int STATUS_HEIGHT = 33;
        public const float PANEL_WIDTH = 0.25f;

        public void UpdatePanel(MultiUserPanel panel)
        {
            statusSprite.Text = statusString;
        }

        public MultiUserPanel(string user = "Unknown")
        {
            userName = user;

            RelativeSizeAxes = Axes.X;
            Height = PANEL_HEIGHT;
            Width = PANEL_WIDTH;
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 0;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(34,34,34, 255),
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = STATUS_HEIGHT,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Colour = new Color4(17, 136, 170, 128),
                },
                new Container //Avatar
                {
                    Masking = true,
                    CornerRadius = 5,
                    Width = 50,
                    Height = 50,
                    Margin = new MarginPadding { Top = 9, Left = 10 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Gray,
                        }
                    }
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirections.Vertical,

                    Children = new Drawable[]
                    {
                        userSprite = new OsuSpriteText
                        {
                            Text = userName,
                            TextSize = 16,
                            Font = @"Exo2.0-RegularItalic",
                            Margin = new MarginPadding { Top = 13, Bottom = 10, Left = 70 }
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Direction = FlowDirections.Horizontal,
                            Padding = new MarginPadding { Left = 70 },
                            Spacing = new Vector2(5,0),
                            Children = new Drawable[]
                            {

                                new Container //Country Flag
                                {
                                    Masking = true,
                                    CornerRadius = 5,
                                    Width = 30,
                                    Height = 20,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.Gray,
                                        }
                                    }
                                },
                                new Container
                                {
                                    Masking = true,
                                    CornerRadius = 5,
                                    Width = 40,
                                    Height = 20,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.Gray,
                                        }
                                    }
                                },
                                new Container //osu! supporter tag
                                {
                                    Masking = true,
                                    CornerRadius = 10,
                                    Width = 20,
                                    Height = 20,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.Gray,
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                new CircularContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(12, 12),
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    Colour = Color4.White,
                    Margin = new MarginPadding { Left = 80, Bottom = 11 },
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                    },
                },
                statusSprite = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Text = "Waiting in a Room",
                    TextSize = 16,
                    Margin = new MarginPadding { Left = 100, Bottom = 10 },
                }
            };
        }

        public enum UserState
        {
            Host,
            Guest
        }
    }
}
