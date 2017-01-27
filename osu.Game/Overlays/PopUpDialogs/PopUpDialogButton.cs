//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;

namespace osu.Game.Overlays.PopUpDialogs
{
    public class PopUpDialogButton : ClickableContainer
    {
        public string Text
        {
            get { return spriteText?.Text; }
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        //Colours
        public Color4 BackgroundColour
        {
            get { return backgroundBox.Colour; }
            set
            {
                if (backgroundBox != null)
                    backgroundBox.Colour = value;
            }
        }

        public new Color4 Colour
        {
            get { return mainBox.Colour; }
            set
            {
                mainBox.Colour = value;
            }
        }

        //Sizing
        public float BackgroundWidth
        {
            get { return backgroundBox.Width; }
            set
            {
                backgroundBox.Width = value;
            }
        }
        public override float Width
        {
            get { return mainBox.Width; }
            set
            {
                mainBox.Width = value;
            }
        }

        public float BackgroundHeight
        {
            get { return backgroundBox.Height; }
            set
            {
                backgroundBox.Height = value;
            }
        }

        public override float Height
        {
            get { return mainBox.Height; }
            set
            {
                mainBox.Height = value;
            }
        }

        const float transition_time = 400;

        //Component Drawables
        private SpriteText spriteText;
        private Box mainBox;
        private Box backgroundBox;

        public PopUpDialogButton()
        {
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                backgroundBox = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Shear = new Vector2(0.15f, 0),
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = new Color4(0, 0, 0, 60),
                        Radius = 10,
                    },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        mainBox = new Box
                        {
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                    }
                },
                spriteText = new SpriteText
                {
                    Font = @"Exo2.0-Bold",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shadow = true,
                    ShadowColour = new Color4(0f, 0f, 0f, 1f),
                }
            };
        }

        protected override bool OnClick(InputState state)
        {
            Flush();
            FadeOutFromOne(transition_time);
            Delay(transition_time);
            FadeInFromZero(transition_time);
            base.OnClick(state);

            return true;
        }
    }
}
