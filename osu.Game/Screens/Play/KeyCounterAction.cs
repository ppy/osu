// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;

namespace osu.Game.Screens.Play
{
    public class KeyCounterAction<T> : KeyCounter, IKeyBindingHandler<T>
        where T : struct
    {
        public T Action { get; }

        public KeyCounterAction(T action) : base($"B{(int)(object)action + 1}")
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
