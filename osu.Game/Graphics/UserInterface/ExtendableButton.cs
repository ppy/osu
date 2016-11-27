// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class ExtendableButton : ClickableContainer
    {
        public Color4 BGColour
        {
            get { return bgBox.Colour; }
            set { bgBox.Colour = value; }
        }
        public new Color4 Colour
        {
            get { return textBox.Colour; }
            set { textBox.Colour = value; }
        }
        public new Vector2 Shear
        {
            get { return textBox.Shear; }
            set { textBox.Shear = value; }
        }
        public string Text
        {
            get { return spriteText?.Text; }
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        private Box bgBox;
        private Box textBox;
        public Container textContainer;
        private SpriteText spriteText;

        public Vector2 ExtendLenght = new Vector2(80,0); // this one can be defaulted
        public Vector2 InitialExtendLenght/* = new Vector2(40, 0)*/; // but this one unfortunately cant

        public ExtendableButton(float initialExtendLenghtX = 20, float initialExtendLenghtY = 0)
        {
            InitialExtendLenght = new Vector2(initialExtendLenghtX, initialExtendLenghtY); // because it just wont use defined settings when creating

            Children = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                textContainer = new Container
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Position = Position + InitialExtendLenght,
                    Children = new Drawable[]
                    {
                        textBox = new Box
                        {
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.Both,
                            Shear = new Vector2(0.1f, 0), // should be relative?
                            EdgeSmoothness = new Vector2(1, 1), // should be based on which side is being cut?
                        },
                        spriteText = new SpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    }
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            textContainer.ClearTransformations();
            textContainer.MoveTo(Position + ExtendLenght, 150, EasingTypes.OutElastic);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            base.OnHoverLost(state);
            textContainer.ClearTransformations();
            textContainer.MoveTo(Position + InitialExtendLenght, 150, EasingTypes.OutElastic);
        }

        protected override bool OnClick(InputState state)
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            Add(flash);

            flash.Colour = textBox.Colour;
            flash.BlendingMode = BlendingMode.Additive;
            flash.Alpha = 0.3f;
            flash.FadeOutFromOne(200);
            flash.Expire();

            return base.OnClick(state);
        }
    }
}
