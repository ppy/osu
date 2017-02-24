// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialogButton : ClickableContainer
    {
        private float height = 50;
        private float foreground_shear = 0.2f;

        private Box background, foreground;
        private Triangles triangles;
        private OsuSpriteText label;

        public string Title
        {
            get
            {
                return label.Text;
            }
            set
            {
                label.Text = value;
            }
        }

        public Color4 ForegroundColour
        {
            get
            {
                return foreground.Colour;
            }
            set
            {
                foreground.Colour = value;
            }
        }

        public Color4 BackgroundColour
        {
            get
            {
                return background.Colour;
            }
            set
            {
                background.Colour = value;
            }
        }

        public Color4 TrianglesColourLight
        {
            get
            {
                return triangles.ColourLight;
            }
            set
            {
                triangles.ColourLight = value;
            }
        }

        public Color4 TrianglesColourDark
        {
            get
            {
                return triangles.ColourDark;
            }
            set
            {
                triangles.ColourDark = value;
            }
        }

        public PopupDialogButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        },
                        triangles = new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                },
                new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Shear = new Vector2(foreground_shear, 0f),
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(50),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        foreground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                        label = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Shear = new Vector2(-foreground_shear, 0f),
                            Text = @"Button",
                            Font = @"Exo2.0-Bold",
                            TextSize = 18,
                        },
                    },
                },
            };
        }
    }

    public class PopupDialogOKButton : PopupDialogButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.PinkDark;
            ForegroundColour = colours.Pink;
            TrianglesColourDark = colours.PinkDarker;
            TrianglesColourLight = colours.Pink;
        }
    }

    public class PopupDialogCancelButton : PopupDialogButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.BlueDark;
            ForegroundColour = colours.Blue;
            TrianglesColourDark = colours.BlueDarker;
            TrianglesColourLight = colours.Blue;
        }
    }
}
