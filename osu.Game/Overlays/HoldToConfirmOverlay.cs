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

        /// <summary>
        /// Whether the overlay should be allowed to return from a fired state.
        /// </summary>
        protected virtual bool AllowMultipleFires => false;

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

        protected void BeginConfirm()
        {
            if (!AllowMultipleFires && fired) return;
            overlay.FadeIn(activate_delay * (1 - overlay.Alpha), Easing.Out).OnComplete(_ =>
            {
                Action?.Invoke();
                fired = true;
            });
        }

        protected void AbortConfirm()
        {
            if (!AllowMultipleFires && fired) return;
            overlay.FadeOut(fadeout_delay, Easing.Out);
        }
    }
}
