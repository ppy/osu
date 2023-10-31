// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Play.HUD
{
    public partial class KeyCounterActionTrigger<T> : InputTrigger, IKeyBindingHandler<T>
        where T : struct
    {
        public T Action { get; }

        public KeyCounterActionTrigger(T action)
            : base($"B{(int)(object)action + 1}")
        {
            Action = action;
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            if (!EqualityComparer<T>.Default.Equals(e.Action, Action))
                return false;

            Activate(Clock.Rate >= 0);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            if (!EqualityComparer<T>.Default.Equals(e.Action, Action))
                return;

            Deactivate(Clock.Rate >= 0);
        }
    }
}
