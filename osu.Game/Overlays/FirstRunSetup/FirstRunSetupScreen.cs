// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Overlays.FirstRunSetup
{
    public abstract class FirstRunSetupScreen : Screen
    {
        private const float offset = 100;

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            this
                .FadeInFromZero(500)
                .MoveToX(offset)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            this
                .FadeInFromZero(500)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            this
                .FadeOut(100)
                .MoveToX(offset, 500, Easing.OutQuint);

            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            this
                .FadeOut(100)
                .MoveToX(-offset, 500, Easing.OutQuint);

            base.OnSuspending(next);
        }
    }
}
