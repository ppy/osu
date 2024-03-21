// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An individual key display which is intended to be displayed within a <see cref="KeyCounterDisplay"/>.
    /// </summary>
    public abstract partial class KeyCounter : Container
    {
        /// <summary>
        /// The <see cref="InputTrigger"/> which activates and deactivates this <see cref="KeyCounter"/>.
        /// </summary>
        public readonly InputTrigger Trigger;

        /// <summary>
        /// The current count of registered key presses.
        /// </summary>
        public IBindable<int> CountPresses => Trigger.ActivationCount;

        /// <summary>
        /// Whether this <see cref="KeyCounter"/> is currently in the "activated" state because the associated key is currently pressed.
        /// </summary>
        protected readonly Bindable<bool> IsActive = new BindableBool();

        protected KeyCounter(InputTrigger trigger)
        {
            Trigger = trigger;

            Trigger.OnActivate += Activate;
            Trigger.OnDeactivate += Deactivate;
        }

        protected virtual void Activate(bool forwardPlayback = true)
        {
            IsActive.Value = true;
        }

        protected virtual void Deactivate(bool forwardPlayback = true)
        {
            IsActive.Value = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Trigger.OnActivate -= Activate;
            Trigger.OnDeactivate -= Deactivate;
        }
    }
}
