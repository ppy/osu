// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Screens;

namespace osu.Game.Overlays.FirstRunSetup
{
    public abstract class FirstRunSetupScreen : Screen
    {
        [Resolved]
        protected FirstRunSetupOverlay Overlay { get; private set; }

        protected override bool OnClick(ClickEvent e) => true;

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            this
                .FadeInFromZero(500)
                .MoveToX(100)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            this
                .FadeInFromZero(500)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            this
                .FadeOut(100)
                .MoveToX(-100, 500, Easing.OutQuint);
        }
    }
}
