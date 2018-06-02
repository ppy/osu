// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Screens.Edit
{
    public enum SampleSet
    {
        [Description(@"Normal")]
        Normal = 0,
        [Description(@"Soft")]
        Soft = 1,
        [Description(@"Drum")]
        Drum = 2,
    }
}
