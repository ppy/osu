// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Play
{
    public class KeyCounterAction<T> : KeyCounter
        where T : struct
    {
        public T Action { get; }

        public KeyCounterAction(T action)
            : base($"B{(int)(object)action + 1}")
        {
            Action = action;
        }

        public bool OnPressed(T action, bool forwards)
        {
            if (!action.Equals(Action))
                return false;

            IsLit = true;
            if (forwards)
                Increment();
            return false;
        }

        public bool OnReleased(T action, bool forwards)
        {
            if (!action.Equals(Action))
                return false;

            IsLit = false;
            if (!forwards)
                Decrement();
            return false;
        }
    }
}
