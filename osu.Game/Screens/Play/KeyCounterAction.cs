// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;

namespace osu.Game.Screens.Play
{
    public class KeyCounterAction<T> : KeyCounter, IKeyBindingHandler<T>
        where T : struct
    {
        public T Action { get; }

        public KeyCounterAction(T action)
            : base($"B{(int)(object)action + 1}")
        {
            Action = action;
        }

        public bool OnPressed(T action)
        {
            if (action.Equals(Action)) IsLit = true;
            return false;
        }

        public bool OnReleased(T action)
        {
            if (action.Equals(Action)) IsLit = false;
            return false;
        }
    }
}
