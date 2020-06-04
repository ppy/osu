// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Select
{
    public class FooterButtonRandom : FooterButton
    {
        public Action NextRandom { get; set; }
        public Action PreviousRandom { get; set; }

        private readonly SpriteText secondaryText;
        private bool rewindSearch;

        public FooterButtonRandom()
        {
            TextContainer.Add(secondaryText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = @"rewind",
                Alpha = 0,
            });

            // force both text sprites to always be present to avoid width flickering while they're being swapped out
            SpriteText.AlwaysPresent = secondaryText.AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Green;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"random";
        }

        private void updateText()
        {
            if (rewindSearch)
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

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SelectPreviousRandom:
                    rewindSearch = true;
                    Action = PreviousRandom;
                    updateText();
                    Click();
                    return true;

                case GlobalAction.SelectNextRandom:
                    Action = NextRandom;
                    updateText();
                    Click();
                    return true;
            }

            return false;
        }

        public override void OnReleased(GlobalAction action)
        {
            if (action == GlobalAction.SelectPreviousRandom)
            {
                rewindSearch = false;
                updateText();
            }
        }
    }
}
