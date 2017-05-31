// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Configuration
{
   public enum SelectionRandomType
    {
        [Description("Random")]
        Random,
        [Description("Never repeat")]
        RandomPermutation
    }
}
