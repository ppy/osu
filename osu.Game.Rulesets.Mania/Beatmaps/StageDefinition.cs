// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Defines properties for each stage in a <see cref="ManiaPlayfield"/>.
    /// </summary>
    public struct StageDefinition
    {
        /// <summary>
        /// The number of <see cref="Column"/>s which this stage contains.
        /// </summary>
        public int Columns;
    }
}
