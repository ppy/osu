// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Defines properties for each grouping of <see cref="Column"/>s in a <see cref="ManiaPlayfield"/>.
    /// </summary>
    public struct GroupDefinition
    {
        /// <summary>
        /// The number of <see cref="Column"/>s which this grouping contains.
        /// </summary>
        public int Columns;
    }
}
