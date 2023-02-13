// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public partial class KeyCounterAction<T> : KeyCounter.Trigger
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
            if (!EqualityComparer<T>.Default.Equals(action, Action))
                return false;

            Lit(forwards);
            return false;
        }

        public void OnReleased(T action, bool forwards)
        {
            if (!EqualityComparer<T>.Default.Equals(action, Action))
                return;

            Unlit(forwards);
        }
    }
}
