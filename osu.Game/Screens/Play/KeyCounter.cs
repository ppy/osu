// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    public abstract partial class KeyCounter : Container
    {
        public readonly InputTrigger Trigger;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        private readonly Bindable<int> countPresses = new BindableInt
        {
            MinValue = 0
        };

        public bool IsCounting { get; set; } = true;

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

        protected Bindable<bool> IsLit = new BindableBool();

        public void Increment()
        {
            if (!IsCounting)
                return;

            countPresses.Value++;
        }

        public void Decrement()
        {
            if (!IsCounting)
                return;

            countPresses.Value--;
        }

        protected virtual void Activate(bool increment = true)
        {
            IsLit.Value = true;
            if (increment)
                Increment();
        }

        protected virtual void Deactivate(bool preserve = true)
        {
            IsLit.Value = false;
            if (!preserve)
                Decrement();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Trigger.OnActivate -= Activate;
            Trigger.OnDeactivate -= Deactivate;
        }

        public abstract partial class InputTrigger : Component
        {
            public event Action<bool>? OnActivate;
            public event Action<bool>? OnDeactivate;

            protected InputTrigger(string name)
            {
                Name = name;
            }

            protected void Activate(bool forwardPlayback = true) => OnActivate?.Invoke(forwardPlayback);

            protected void Deactivate(bool forwardPlayback = true) => OnDeactivate?.Invoke(forwardPlayback);
        }
    }
}
