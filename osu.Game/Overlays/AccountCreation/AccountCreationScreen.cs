// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Overlays.AccountCreation
{
    public abstract class AccountCreationScreen : Screen
    {
        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Content.FadeOut().Delay(200).FadeIn(200);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            Content.FadeIn(200);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Content.FadeOut(200);
        }
    }
}
