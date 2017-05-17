// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.MathUtils;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Special converter used for converting from osu!stable beatmaps.
    /// </summary>
    internal class LegacyConverter
    {
        private readonly FastRandom random;

        private readonly int availableColumns;
        private readonly float localXDivisor; 

        private readonly Beatmap beatmap;

        public LegacyConverter(Beatmap beatmap)
        {
            this.beatmap = beatmap;

            int seed = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.DrainRate + beatmap.BeatmapInfo.Difficulty.CircleSize)
                * 20 + (int)(beatmap.BeatmapInfo.Difficulty.OverallDifficulty * 41.2) + (int)Math.Round(beatmap.BeatmapInfo.Difficulty.ApproachRate);

            availableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);
            localXDivisor = 512.0f / availableColumns;
        }

        public IEnumerable<ManiaHitObject> Convert(HitObject original)
        {
            if (beatmap.BeatmapInfo.RulesetID == 3)
                yield return generateSpecific(original);
        }

        private ManiaHitObject generateSpecific(HitObject original)
        {
            var endTimeData = original as IHasEndTime;
            var positionData = original as IHasXPosition;

            int column = getColumn(positionData?.X ?? 0);

            if (endTimeData != null)
            {
                return new HoldNote
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    Duration = endTimeData.Duration,
                    Column = column,
                };
            }

            return new Note
            {
                StartTime = original.StartTime,
                Samples = original.Samples,
                Column = column
            };
        }

        private int getColumn(float position) => MathHelper.Clamp((int)Math.Floor(position / localXDivisor), 0, availableColumns - 1);
    }
}
