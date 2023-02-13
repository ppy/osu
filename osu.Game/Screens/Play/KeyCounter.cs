// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    public abstract partial class KeyCounter : Container
    {
        public readonly InputTrigger Trigger;

        protected Bindable<bool> IsCountingBindable = new BindableBool(true);

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected Bindable<int> PressesCount = new BindableInt
        {
            MinValue = 0
        };

        public bool IsCounting
        {
            get => IsCountingBindable.Value;
            set => IsCountingBindable.Value = value;
        }

        public int CountPresses
        {
            get => PressesCount.Value;
            private set => PressesCount.Value = value;
        }

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

            Trigger.Target = this;
            Name = trigger.Name;
        }

        protected Bindable<bool> IsLit = new BindableBool();

        public void Increment()
        {
            if (!IsCounting)
                return;

            CountPresses++;
        }

        public void Decrement()
        {
            if (!IsCounting)
                return;

            CountPresses--;
        }

        public abstract partial class InputTrigger : Component
        {
            private KeyCounter? target;

            public KeyCounter Target
            {
                set => target = value;
            }

            protected InputTrigger(string name)
            {
                Name = name;
            }

            protected void Lit(bool increment = true)
            {
                if (target == null) return;

                target.IsLit.Value = true;
                if (increment)
                    target.Increment();
            }

            protected void Unlit(bool preserve = true)
            {
                if (target == null) return;

                target.IsLit.Value = false;
                if (!preserve)
                    target.Decrement();
            }
        }
    }
}
