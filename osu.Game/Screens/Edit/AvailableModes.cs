// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Edit
{
    public enum AvailableModes
    {
        [Description(@"All")]
        All = 0,
        [Description(@"osu!taiko")]
        Taiko = 1,
        [Description(@"osu!catch")]
        Catch = 2,
        [Description(@"osu!mania")]
        Mania = 3,
    }
}
