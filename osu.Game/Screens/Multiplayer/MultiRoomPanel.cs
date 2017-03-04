// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiRoomPanel : ClickableContainer
    {
        private bool didClick;
        private string roomName;
        private string hostName;

        private int roomStatus;
        private Color4 statusColour;
        private string statusString;

        private Box sideSprite;
        private OsuSpriteText hostSprite;
        private OsuSpriteText statusSprite;
        private OsuSpriteText roomSprite;

        public const int BORDER_SIZE = 3;
        public const int PANEL_HEIGHT = 90;
        

        public Color4 ColourFree = new Color4(166, 204, 0, 255);
        public Color4 ColourBusy = new Color4(135, 102, 237, 255);

        public int CONTENT_PADDING = 5;

        public bool Clicked
        {
            get { return didClick; }
            set
            {
                didClick = value;
            }
        }

        public int Status
        {
            get { return roomStatus; }
            set
            {
                roomStatus = value;
                if (roomStatus == 0)
                {
                    statusColour = ColourFree;
                    statusString = "Welcoming Players";

                    UpdatePanel(this);
                }
                else
                {
                    statusColour = ColourBusy;
                    statusString = "Now Playing";

                    UpdatePanel(this);
                }
            }
        }

        public void UpdatePanel(MultiRoomPanel panel)
        {
            panel.BorderColour = statusColour;
            panel.sideSprite.Colour = statusColour;

            statusSprite.Colour = statusColour;
            statusSprite.Text = statusString;
        }

        public MultiRoomPanel(string matchName = "Room Name", string host = "Undefined", int status = 0)
        {
            roomName = matchName;
            hostName = host;
            roomStatus = status;

            if (status == 0)
            {
                statusColour = ColourFree;
                statusString = "Welcoming Players";
            }
            else
            {
                statusColour = ColourBusy;
                statusString = "Now Playing";
            }

            RelativeSizeAxes = Axes.X;
            Height = PANEL_HEIGHT;
            Masking = true;
            CornerRadius = 5;
            BorderThickness = 0;
            BorderColour = statusColour;
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
                sideSprite = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 5,
                    Colour = statusColour,
                },
                /*new Box //Beatmap img 
                {

                },*/
                new Background(@"Backgrounds/bg4")
                {
                    RelativeSizeAxes = Axes.Both,
                }
                ,
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirections.Vertical,
                    Size = new Vector2(0.75f,1),

                    Children = new Drawable[]
                    {
                        roomSprite = new OsuSpriteText
                        {
                            Text = roomName,
                            TextSize = 18,
                            Margin = new MarginPadding { Top = CONTENT_PADDING },
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Direction = FlowDirections.Horizontal,
                            Spacing = new Vector2(5,0),
                            Children = new Drawable[]
                            {
                                
                                
                                new Container
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
                                            Colour = new Color4(173,56,126,255),
                                        }
                                    }
                                },
                                new OsuSpriteText
                                {
                                    Text = "hosted by",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    TextSize = 14,
                                },
                                hostSprite = new OsuSpriteText
                                {
                                    Text = hostName,
                                    Font = @"Exo2.0-Bold",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Shear = new Vector2(0.1f,0),
                                    Colour = new Color4(69,179,222,255),
                                    TextSize = 14,
                                },
                                new OsuSpriteText
                                {
                                    Text = "#6895 - #50024",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    TextSize = 14,
                                    Margin = new MarginPadding { Left = 80 },
                                    Colour = new Color4(153,153,153,255),
                                }
                            }
                        },
                        statusSprite = new OsuSpriteText
                        {
                            Text = statusString,
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Colour = statusColour,
                            Margin = new MarginPadding { Top = 10 }
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Direction = FlowDirections.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Platina",
                                    Font = @"Exo2.0-Bold",
                                    TextSize = 14,
                                    Shear = new Vector2(0.1f,0),
                                    Colour = new Color4(153,153,153,255),
                                },
                                new OsuSpriteText
                                {
                                    Text = " - " + "Maaya Sakamoto",
                                    TextSize = 14,
                                    Shear = new Vector2(0.1f,0),
                                    Colour = new Color4(153,153,153,255),
                                }
                            }
                        },
                    }
                },
            };
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            BorderThickness = 3;
            didClick = true;
            return base.OnMouseUp(state, args);
        }
    }
}