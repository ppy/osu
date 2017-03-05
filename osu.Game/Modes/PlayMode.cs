// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel;

namespace osu.Game.Modes
{
    public static class PlayMode
    {
        public static Dictionary<int,string> Description = new Dictionary<int,string>();

        public const int Osu = 0;
        public const int Taiko = 1;
        public const int Catch = 2;
        public const int Mania = 3;
    }
}
