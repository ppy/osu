// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class KeyCounterController : CompositeComponent, IAttachableSkinComponent
    {
        public readonly Bindable<bool> IsCounting = new BindableBool(true);

        private Receptor? receptor;

        public event Action<InputTrigger>? OnNewTrigger;

        private readonly Container<InputTrigger> triggers;

        public IReadOnlyList<InputTrigger> Triggers => triggers;

        public KeyCounterController()
        {
            InternalChild = triggers = new Container<InputTrigger>();
        }

        public void Add(InputTrigger trigger)
        {
            triggers.Add(trigger);
            trigger.IsCounting.BindTo(IsCounting);
            OnNewTrigger?.Invoke(trigger);
        }

        public void AddRange(IEnumerable<InputTrigger> inputTriggers) => inputTriggers.ForEach(Add);

        /// <summary>
        /// Sets a <see cref="Receptor"/> that will populate keybinding events to this <see cref="KeyCounterController"/>.
        /// </summary>
        /// <param name="receptor">The receptor to set</param>
        /// <exception cref="InvalidOperationException">When a <see cref="Receptor"/> is already active on this <see cref="KeyCounterDisplay"/></exception>
        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        /// <summary>
        /// Clears any <see cref="KeyCounterController.Receptor"/> active
        /// </summary>
        public void ClearReceptor()
        {
            receptor = null;
        }

        public override bool HandleNonPositionalInput => receptor == null;

        public override bool HandlePositionalInput => receptor == null;

        public partial class Receptor : Drawable
        {
            protected readonly KeyCounterController Target;

            public Receptor(KeyCounterController target)
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
                        return Target.TriggerEvent(e);
                }

                return base.Handle(e);
            }
        }
    }
}
