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

        private bool rewindSearch;

        public FooterButtonRandom() { }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Green;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"random";
            Action = () =>
            {
                if (rewindSearch)
                {
                    PreviousRandom.Invoke();
                }
                else
                {
                    NextRandom.Invoke();
                }
            };
        }

        public override bool OnPressed(GlobalAction action)
        {
            rewindSearch = action == GlobalAction.SelectPreviousRandom;

            if (action != GlobalAction.SelectNextRandom && !rewindSearch)
            {
                return false;
            }

            Click();
            return true;
        }

        public override void OnReleased(GlobalAction action)
        {
            if (action == GlobalAction.SelectPreviousRandom)
            {
                rewindSearch = false;
            }
        }
    }
}
