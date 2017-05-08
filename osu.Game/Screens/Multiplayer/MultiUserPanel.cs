// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

using osu.Game.Users;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiUserPanel : ClickableContainer
    {
        private bool host;
        public bool Host
        {
            get { return host; }
            set
            {
                if (host == value)
                    return;

                host = value;
                switch(host)
                {
                    case true:
                        statusSprite.Text = STRING_HOST;
                        break;
                    case false:
                        statusSprite.Text = STRING_GUEST;
                        break;
                }
            }
        }
        
        private OsuSpriteText userSprite;
        private OsuSpriteText statusSprite;
        private Box background;
        private Box statusBox;

        private const int PANEL_HEIGHT = 100;
        private const int STATUS_HEIGHT = 33;
        private const float PANEL_WIDTH = 0.25f;
        private const string STRING_GUEST = "Waiting in a Room";
        private const string STRING_HOST = "Hosting a room";

        public MultiUserPanel(User user = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = PANEL_HEIGHT;
            Width = PANEL_WIDTH;
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 0;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = new Color4(0, 0, 0, 40),
                Radius = 5,
            };
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                statusBox = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = STATUS_HEIGHT,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                new Avatar(user)
                {
                    Masking = true,
                    CornerRadius = 5,
                    Width = 50,
                    Height = 50,
                    Margin = new MarginPadding { Top = 9, Left = 10 },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Full,
                    Children = new Drawable[]
                    {
                        userSprite = new OsuSpriteText
                        {
                            Text = user.Username,
                            TextSize = 16,
                            Font = @"Exo2.0-RegularItalic",
                            Margin = new MarginPadding { Top = 13, Bottom = 10, Left = 70 }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = 70 },
                            Spacing = new Vector2(5,0),
                            Children = new Drawable[]
                            {
                                new DrawableFlag(user?.Country?.FlagName ?? "__")
                                {
                                    Masking = true,
                                    CornerRadius = 5,
                                    Width = 30,
                                    Height = 20,
                                    FlagName = user.Country.FlagName,
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
                    Masking = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(12, 12),
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    Colour = Color4.White,
                    Margin = new MarginPadding { Left = 60, Bottom = 11 },
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
                    Text = STRING_GUEST,
                    TextSize = 16,
                    Margin = new MarginPadding { Left = 80, Bottom = 10 },
                }
            };
        }
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
            statusBox.Colour = colours.BlueDarker;
        }
    }
}
