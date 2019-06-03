// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class FooterButtonRandom : FooterButton
    {
        private readonly SpriteText secondaryText;
        private bool secondaryActive;

        public FooterButtonRandom()
        {
            TextContainer.Add(secondaryText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = @"rewind",
                Alpha = 0
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Green;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"random";
            Hotkey = Key.F2;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            secondaryActive = e.ShiftPressed;
            updateText();
            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            secondaryActive = e.ShiftPressed;
            updateText();
            return base.OnKeyUp(e);
        }

        private void updateText()
        {
            if (secondaryActive)
            {
                SpriteText.FadeOut(120, Easing.InQuad);
                secondaryText.FadeIn(120, Easing.InQuad);
            }
            else
            {
                SpriteText.FadeIn(120, Easing.InQuad);
                secondaryText.FadeOut(120, Easing.InQuad);
            }
        }
    }
}
