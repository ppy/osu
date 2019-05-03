// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using System;

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
