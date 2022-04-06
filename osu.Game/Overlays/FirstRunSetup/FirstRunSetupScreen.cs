// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;

namespace osu.Game.Overlays.FirstRunSetup
{
    public abstract class FirstRunSetupScreen : Screen
    {
        [Resolved]
        protected FirstRunSetupOverlay Overlay { get; private set; }

        protected Container Content { get; private set; }

        protected FirstRunSetupScreen()
        {
            InternalChildren = new Drawable[]
            {
                Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                },
            };
        }

        protected override bool OnClick(ClickEvent e) => true;

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
