//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Modes
{
    public enum PlayMode
    {
        [Description(@"osu!")]
        Osu = 0,
        [Description(@"osu!taiko")]
        Taiko = 1,
        [Description(@"osu!catch")]
        Catch = 2,
        [Description(@"osu!mania")]
        Mania = 3
    }
}
