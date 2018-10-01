// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Play
{
    public class KeyCounterState
    {
        public KeyCounterState(double time, int count)
        {
            Time = time;
            Count = count;
        }

        public readonly double Time;
        public readonly int Count;
    }
}
