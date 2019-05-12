// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Overlays.AccountCreation
{
    public abstract class AccountCreationScreen : Screen
    {
        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            this.FadeOut().Delay(200).FadeIn(200);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            this.FadeIn(200);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            this.FadeOut(200);
        }
    }
}
