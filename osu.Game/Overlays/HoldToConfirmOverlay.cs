// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An overlay which will display a black screen that dims over a period before confirming an exit action.
    /// Action is BYO (derived class will need to call <see cref="BeginConfirm"/> and <see cref="AbortConfirm"/> from a user event).
    /// </summary>
    public abstract class HoldToConfirmOverlay : Container
    {
        public Action Action;

        private Box overlay;

        private const int activate_delay = 400;
        private const int fadeout_delay = 200;

        private bool fired;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                overlay = new Box
                {
                    Alpha = 0,
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected void BeginConfirm() => overlay.FadeIn(activate_delay, Easing.Out);

        protected void AbortConfirm() => overlay.FadeOut(fadeout_delay, Easing.Out);

        protected override void Update()
        {
            base.Update();
            if (!fired && overlay.Alpha == 1)
            {
                fired = true;
                Action?.Invoke();
            }
        }
    }
}
