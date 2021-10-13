// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class FooterButtonRandom : FooterButton
    {
        public Action NextRandom { get; set; }
        public Action PreviousRandom { get; set; }

        private bool rewindSearch;

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
                    const double fade_time = 500;

                    OsuSpriteText rewindSpriteText;

                    TextContainer.Add(rewindSpriteText = new OsuSpriteText
                    {
                        Alpha = 0,
                        Text = @"rewind",
                        AlwaysPresent = true, // make sure the button is sized large enough to always show this
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });

                    rewindSpriteText.FadeOutFromOne(fade_time, Easing.In);
                    rewindSpriteText.MoveTo(Vector2.Zero).MoveTo(new Vector2(0, 10), fade_time, Easing.In);
                    rewindSpriteText.Expire();

                    SpriteText.FadeInFromZero(fade_time, Easing.In);

                    PreviousRandom.Invoke();
                }
                else
                {
                    NextRandom.Invoke();
                }
            };
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            rewindSearch = e.Action == GlobalAction.SelectPreviousRandom;

            if (e.Action != GlobalAction.SelectNextRandom && e.Action != GlobalAction.SelectPreviousRandom)
            {
                return false;
            }

            TriggerClick();
            return true;
        }

        public override void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.SelectPreviousRandom)
            {
                rewindSearch = false;
            }
        }
    }
}
