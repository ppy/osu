// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Play
{
    public class KeyCounterMemento
    {
        public KeyCounterMemento(double currentTime, int countPresses)
        {
            CurrentTime = currentTime;
            CountPresses = countPresses;
        }

        public double CurrentTime { get; }
        public int CountPresses { get; }
    }
}
