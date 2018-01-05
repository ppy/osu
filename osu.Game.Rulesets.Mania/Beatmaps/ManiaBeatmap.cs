﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        /// The definitions for each stage in a <see cref="ManiaPlayfield"/>.
        /// </summary>
        public readonly List<StageDefinition> Stages = new List<StageDefinition>();

        /// <summary>
        /// Total number of columns represented by all stages in this <see cref="ManiaBeatmap"/>.
        /// </summary>
        public int TotalColumns => Stages.Sum(g => g.Columns);

        /// <summary>
        /// Creates a new <see cref="ManiaBeatmap"/>.
        /// </summary>
        /// <param name="initialStage">The initial stage.</param>
        public ManiaBeatmap(StageDefinition initialStage)
        {
            Stages.Add(initialStage);
        }
    }
}
