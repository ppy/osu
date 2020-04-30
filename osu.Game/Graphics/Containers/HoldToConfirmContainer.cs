// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;

namespace osu.Game.Graphics.Containers
{
    public abstract class HoldToConfirmContainer : Container
    {
        public Action Action;

        private const int fadeout_delay = 200;

        /// <summary>
        /// Whether currently in a fired state (and the confirm <see cref="Action"/> has been sent).
        /// </summary>
        public bool Fired { get; private set; }

        private bool confirming;

        /// <summary>
        /// Whether the overlay should be allowed to return from a fired state.
        /// </summary>
        protected virtual bool AllowMultipleFires => false;

        public Bindable<double> Progress = new BindableDouble();

        private Bindable<float> holdActivationDelay;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            holdActivationDelay = config.GetBindable<float>(OsuSetting.UIHoldActivationDelay);
        }

        protected void BeginConfirm()
        {
            if (confirming || (!AllowMultipleFires && Fired)) return;

            confirming = true;

            this.TransformBindableTo(Progress, 1, holdActivationDelay.Value * (1 - Progress.Value), Easing.Out).OnComplete(_ => Confirm());
        }

        protected virtual void Confirm()
        {
            Action?.Invoke();
            Fired = true;
        }

        protected void AbortConfirm()
        {
            if (!AllowMultipleFires && Fired) return;

            confirming = false;
            Fired = false;

            this.TransformBindableTo(Progress, 0, fadeout_delay, Easing.Out);
        }
    }
}
