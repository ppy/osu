// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Keeps track of key press counts for a current play session, exposing bindable counts which can
    /// be used for display purposes.
    /// </summary>
    public partial class InputCountController : Component
    {
        public readonly Bindable<bool> IsCounting = new BindableBool(true);

        private readonly BindableList<InputTrigger> triggers = new BindableList<InputTrigger>();

        public IBindableList<InputTrigger> Triggers => triggers;

        public void AddRange(IEnumerable<InputTrigger> triggers) => triggers.ForEach(Add);

        public void Add(InputTrigger trigger)
        {
            // Note that these triggers are not added to the hierarchy here. It is presumed they are added externally at a
            // more correct location (ie. inside a RulesetInputManager).
            triggers.Add(trigger);
            trigger.IsCounting.BindTo(IsCounting);
        }
    }
}
