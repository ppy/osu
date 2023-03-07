// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An individual key display which is intended to be displayed within a <see cref="KeyCounterDisplay"/>.
    /// </summary>
    public abstract partial class KeyCounter : Container
    {
        public readonly InputTrigger Trigger;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        private readonly Bindable<int> countPresses = new BindableInt
        {
            MinValue = 0
        };

        public Bindable<bool> IsCounting { get; } = new BindableBool(true);

        public IBindable<int> CountPresses => countPresses;

        protected KeyCounter(InputTrigger trigger)
        {
            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                Trigger = trigger,
            };

            Trigger.OnActivate += Activate;
            Trigger.OnDeactivate += Deactivate;

            Name = trigger.Name;
        }

        protected readonly Bindable<bool> IsActive = new BindableBool();

        private void increment()
        {
            if (!IsCounting.Value)
                return;

            countPresses.Value++;
        }

        private void decrement()
        {
            if (!IsCounting.Value)
                return;

            countPresses.Value--;
        }

        protected virtual void Activate(bool forwardPlayback = true)
        {
            IsActive.Value = true;
            if (forwardPlayback)
                increment();
        }

        protected virtual void Deactivate(bool forwardPlayback = true)
        {
            IsActive.Value = false;
            if (!forwardPlayback)
                decrement();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Trigger.OnActivate -= Activate;
            Trigger.OnDeactivate -= Deactivate;
        }
    }
}
