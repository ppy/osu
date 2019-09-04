// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public abstract class HoldToConfirmContainer : Container
    {
        public Action Action;

        private const int default_activation_delay = 200;
        private const int fadeout_delay = 200;

        private readonly double activationDelay;

        private bool fired;
        private bool confirming;

        /// <summary>
        /// Whether the overlay should be allowed to return from a fired state.
        /// </summary>
        protected virtual bool AllowMultipleFires => false;

        public Bindable<double> Progress = new BindableDouble();

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="activationDelay">The time requried before an action is confirmed.</param>
        protected HoldToConfirmContainer(double activationDelay = default_activation_delay)
        {
            this.activationDelay = activationDelay;
        }

        protected void BeginConfirm()
        {
            if (confirming || (!AllowMultipleFires && fired)) return;

            confirming = true;

            this.TransformBindableTo(Progress, 1, activationDelay * (1 - Progress.Value), Easing.Out).OnComplete(_ => Confirm());
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
