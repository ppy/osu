// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public abstract class HoldToConfirmContainer : Container
    {
        public Action Action;

        private const int activate_delay = 400;
        private const int fadeout_delay = 200;

        private bool fired;
        private bool confirming;

        /// <summary>
        /// Whether the overlay should be allowed to return from a fired state.
        /// </summary>
        protected virtual bool AllowMultipleFires => false;

        public Bindable<double> Progress = new BindableDouble();

        protected void BeginConfirm()
        {
            if (confirming || !AllowMultipleFires && fired) return;

            confirming = true;

            this.TransformBindableTo(Progress, 1, activate_delay * (1 - Progress.Value), Easing.Out).OnComplete(_ => Confirm());
        }

        protected virtual void Confirm()
        {
            Action?.Invoke();
            fired = true;
        }

        protected void AbortConfirm()
        {
            if (!AllowMultipleFires && fired) return;

            confirming = false;

            this.TransformBindableTo(Progress, 0, fadeout_delay, Easing.Out);
        }
    }
}
