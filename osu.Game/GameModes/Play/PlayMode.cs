//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using System.ComponentModel;

namespace osu.Game.GameModes.Play
{
    public enum PlayMode
    {
        [Description(@"osu!")]
        Osu = 0,
        [Description(@"taiko")]
        Taiko = 1,
        [Description(@"catch")]
        Catch = 2,
        [Description(@"mania")]
        Mania = 3
    }
}
