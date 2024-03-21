// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An event trigger which can be used with <see cref="KeyCounter"/> to create visual tracking of button/key presses.
    /// </summary>
    public abstract partial class InputTrigger : Component
    {
        /// <summary>
        /// Callback to invoke when the associated input has been activated.
        /// </summary>
        /// <param name="forwardPlayback">Whether gameplay is progressing in the forward direction time-wise.</param>
        public delegate void OnActivateCallback(bool forwardPlayback);

        /// <summary>
        /// Callback to invoke when the associated input has been deactivated.
        /// </summary>
        /// <param name="forwardPlayback">Whether gameplay is progressing in the forward direction time-wise.</param>
        public delegate void OnDeactivateCallback(bool forwardPlayback);

        public event OnActivateCallback? OnActivate;
        public event OnDeactivateCallback? OnDeactivate;

        private readonly Bindable<int> activationCount = new BindableInt();
        private readonly Bindable<bool> isCounting = new BindableBool(true);

        /// <summary>
        /// Number of times this <see cref="InputTrigger"/> has been activated.
        /// </summary>
        public IBindable<int> ActivationCount => activationCount;

        /// <summary>
        /// Whether any activation or deactivation of this <see cref="InputTrigger"/> impacts its <see cref="ActivationCount"/>
        /// </summary>
        public IBindable<bool> IsCounting => isCounting;

        protected InputTrigger(string name)
        {
            Name = name;
        }

        protected void Activate(bool forwardPlayback = true)
        {
            if (forwardPlayback && isCounting.Value)
                activationCount.Value++;

            OnActivate?.Invoke(forwardPlayback);
        }

        protected void Deactivate(bool forwardPlayback = true)
        {
            if (!forwardPlayback && isCounting.Value)
                activationCount.Value--;

            OnDeactivate?.Invoke(forwardPlayback);
        }
    }
}
