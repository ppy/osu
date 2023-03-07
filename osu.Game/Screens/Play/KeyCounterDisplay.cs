// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Screens.Play
{
    public abstract partial class KeyCounterDisplay : Container<KeyCounter>
    {
        /// <summary>
        /// Whether the key counter should be visible regardless of the configuration value.
        /// This is true by default, but can be changed.
        /// </summary>
        public Bindable<bool> AlwaysVisible { get; } = new Bindable<bool>(true);

        public Bindable<bool> IsCounting { get; } = new BindableBool(true);

        protected readonly Bindable<bool> ConfigVisibility = new Bindable<bool>();

        protected abstract void UpdateVisibility();

        private Receptor? receptor;

        private readonly Type[] acceptedTypes;

        protected KeyCounterDisplay(params Type[] acceptedTypes)
        {
            this.acceptedTypes = acceptedTypes;
        }

        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        public abstract void AddTrigger(InputTrigger trigger);

        public void AddTriggerRange(IEnumerable<InputTrigger> triggers) => triggers.ForEach(AddTrigger);

        public override void Add(KeyCounter counter)
        {
            if (!checkType(counter))
                throw new InvalidOperationException($"{counter.GetType()} is not a supported counter type. (hint: you may want to use {nameof(AddTrigger)} instead.)");

            base.Add(counter);
            counter.IsCounting.BindTo(IsCounting);
        }

        private bool checkType(KeyCounter counter) => acceptedTypes.Length == 0 || acceptedTypes.Any(t => t.IsInstanceOfType(counter));

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.KeyOverlay, ConfigVisibility);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AlwaysVisible.BindValueChanged(_ => UpdateVisibility());
            ConfigVisibility.BindValueChanged(_ => UpdateVisibility(), true);
        }

        public override bool HandleNonPositionalInput => receptor == null;

        public override bool HandlePositionalInput => receptor == null;

        public partial class Receptor : Drawable
        {
            protected readonly KeyCounterDisplay Target;

            public Receptor(KeyCounterDisplay target)
            {
                RelativeSizeAxes = Axes.Both;
                Depth = float.MinValue;
                Target = target;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool Handle(UIEvent e)
            {
                switch (e)
                {
                    case KeyDownEvent:
                    case KeyUpEvent:
                    case MouseDownEvent:
                    case MouseUpEvent:
                        return Target.InternalChildren.Any(c => c.TriggerEvent(e));
                }

                return base.Handle(e);
            }
        }
    }
}
