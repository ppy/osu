//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelectButton : ClickableContainer
    {
        private const int selection_transition_length = 200;
        private const int click_flash_length = 400;
        private static readonly Vector2 shearing = new Vector2(0.15f, 0);

        public string Text
        {
            get { return spriteText?.Text; }
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        private Color4 deselectedColour;
        public Color4 DeselectedColour
        {
            get { return deselectedColour; }
            set
            {
                deselectedColour = value;
                if(light.Colour != SelectedColour)
                    light.Colour = value;
            }
        }

        public Color4 SelectedColour;

        public new Color4 Colour
        {
            get { return box.Colour; }
            set
            {
                box.Colour = value;
            }
        }

        private SpriteText spriteText;
        private Box box;
        private Box light;
        public PlaySongSelectButtonContainer ButtonContainer;

        public PlaySongSelectButton()
        {
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = shearing,
                    EdgeSmoothness = new Vector2(2, 0),
                    Alpha = 0.8f,
                },
                light = new Box
                {
                    Shear = shearing,
                    Height = 3,
                    EdgeSmoothness = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.X,
                },
                spriteText = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
            Colour = Color4.Black;
        }

        public void OnSelected()
        {
            light.ScaleTo(new Vector2(1, 2), selection_transition_length);
            light.FadeColour(SelectedColour, selection_transition_length);
        }

        public void OnDeselected()
        {
            light.ScaleTo(new Vector2(1, 1), selection_transition_length);
            light.FadeColour(DeselectedColour, selection_transition_length);
        }

        protected override bool OnClick(InputState state)
        {

            ButtonContainer.Select(this);
            box.FlashColour(Color4.White, click_flash_length);
            return base.OnClick(state);
        }

    }
}
