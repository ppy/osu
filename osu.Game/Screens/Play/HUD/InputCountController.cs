// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Keeps track of key press counts for a current play session, exposing bindable counts which can
    /// be used for display purposes.
    /// </summary>
    public partial class InputCountController : CompositeComponent
    {
        public readonly Bindable<bool> IsCounting = new BindableBool(true);

        public event Action<InputTrigger>? OnNewTrigger;

        private readonly Container<InputTrigger> triggers;

        public IReadOnlyList<InputTrigger> Triggers => triggers;

        public InputCountController()
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

        public override bool HandleNonPositionalInput => true;
        public override bool HandlePositionalInput => true;
    }
}
