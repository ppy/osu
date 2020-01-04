// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
