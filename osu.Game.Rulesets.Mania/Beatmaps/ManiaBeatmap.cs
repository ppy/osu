// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmap : Beatmap<ManiaHitObject>
    {
        /// <summary>
        /// The definitions for each grouping in a <see cref="ManiaPlayfield"/>.
        /// </summary>
        public readonly List<GroupDefinition> Groups = new List<GroupDefinition>();

        /// <summary>
        /// Total number of columns represented by all groups in this <see cref="ManiaBeatmap"/>.
        /// </summary>
        public int TotalColumns => Groups.Sum(g => g.Columns);

        /// <summary>
        /// Creates a new <see cref="ManiaBeatmap"/>.
        /// </summary>
        /// <param name="initialGroup">The initial grouping of columns.</param>
        public ManiaBeatmap(GroupDefinition initialGroup)
        {
            Groups.Add(initialGroup);
        }
    }
}
