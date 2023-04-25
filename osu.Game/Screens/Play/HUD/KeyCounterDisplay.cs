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

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A flowing display of all gameplay keys. Individual keys can be added using <see cref="InputTrigger"/> implementations.
    /// </summary>
    public abstract partial class KeyCounterDisplay : CompositeDrawable
    {
        /// <summary>
        /// Whether the key counter should be visible regardless of the configuration value.
        /// This is true by default, but can be changed.
        /// </summary>
        public Bindable<bool> AlwaysVisible { get; } = new Bindable<bool>(true);

        /// <summary>
        /// The <see cref="KeyCounter"/>s contained in this <see cref="KeyCounterDisplay"/>.
        /// </summary>
        public IEnumerable<KeyCounter> Counters => KeyFlow;

        protected abstract FillFlowContainer<KeyCounter> KeyFlow { get; }

        /// <summary>
        /// Whether the actions reported by all <see cref="InputTrigger"/>s within this <see cref="KeyCounterDisplay"/> should be counted.
        /// </summary>
        public Bindable<bool> IsCounting { get; } = new BindableBool(true);

        protected readonly Bindable<bool> ConfigVisibility = new Bindable<bool>();

        protected abstract void UpdateVisibility();

        private Receptor? receptor;

        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        /// <summary>
        /// Add a <see cref="InputTrigger"/> to this display.
        /// </summary>
        public void Add(InputTrigger trigger) => KeyFlow.Add(CreateCounter(trigger));

        /// <summary>
        /// Add a range of <see cref="InputTrigger"/> to this display.
        /// </summary>
        public void AddRange(IEnumerable<InputTrigger> triggers) => triggers.ForEach(Add);

        protected abstract KeyCounter CreateCounter(InputTrigger trigger);

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
