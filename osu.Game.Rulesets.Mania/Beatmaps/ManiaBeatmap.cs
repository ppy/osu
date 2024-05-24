// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public List<StageDefinition> Stages = new List<StageDefinition>();

        /// <summary>
        /// Total number of columns represented by all stages in this <see cref="ManiaBeatmap"/>.
        /// </summary>
        public int TotalColumns => Stages.Sum(g => g.Columns);

        /// <summary>
        /// Creates a new <see cref="ManiaBeatmap"/>.
        /// </summary>
        /// <param name="defaultStage">The initial stages.</param>
        /// <param name="originalTotalColumns">The total number of columns present before any user adjustments. Defaults to the total columns in <paramref name="defaultStage"/>.</param>
        public ManiaBeatmap(StageDefinition defaultStage, int? originalTotalColumns = null)
        {
            Stages.Add(defaultStage);
        }

        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int notes = HitObjects.Count(s => s is Note);
            int holdNotes = HitObjects.Count(s => s is HoldNote);

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Note Count",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    Content = notes.ToString(),
                },
                new BeatmapStatistic
                {
                    Name = @"Hold Note Count",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                    Content = holdNotes.ToString(),
                },
            };
        }

        public StageDefinition GetStageForColumnIndex(int column)
        {
            foreach (var stage in Stages)
            {
                if (column < stage.Columns)
                    return stage;

                column -= stage.Columns;
            }

            throw new ArgumentOutOfRangeException(nameof(column), "Provided index exceeds all available stages");
        }
    }
}
