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

        private ObjectList lastRow = new ObjectList();

        private readonly int availableColumns;
        private readonly float localXDivisor; 

        private readonly Beatmap beatmap;

        public LegacyConverter(Beatmap beatmap)
        {
            this.beatmap = beatmap;

            int seed = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.DrainRate + beatmap.BeatmapInfo.Difficulty.CircleSize)
                * 20 + (int)(beatmap.BeatmapInfo.Difficulty.OverallDifficulty * 41.2) + (int)Math.Round(beatmap.BeatmapInfo.Difficulty.ApproachRate);
            random = new FastRandom(seed);

            availableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);
            localXDivisor = 512.0f / availableColumns;
        }

        public IEnumerable<ManiaHitObject> Convert(HitObject original)
        {
            var maniaOriginal = original as ManiaHitObject;
            if (maniaOriginal != null)
            {
                yield return maniaOriginal;
                yield break;
            }

            if (beatmap.BeatmapInfo.RulesetID == 3)
                yield return generateSpecific(original);
            else
            {
                foreach (ManiaHitObject c in generateConverted(original))
                    yield return c;
            }
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

            if (positionData != null)
            {
                return new Note
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    Column = column
                };
            }

            return null;
        }

        private IEnumerable<ManiaHitObject> generateConverted(HitObject original)
        {
            var endTimeData = original as IHasEndTime;
            var distanceData = original as IHasDistance;
            var positionData = original as IHasPosition;

            ObjectConversion conversion = null;

            if (distanceData != null)
                conversion = new DistanceObjectConversion(original, lastRow, random, beatmap);
            else if (endTimeData != null)
            {
                // Spinner
            }
            else if (positionData != null)
            {
                // Circle
            }

            if (conversion == null)
                yield break;

            ObjectList newRow = conversion.Generate();

            foreach (ManiaHitObject obj in newRow.HitObjects)
                yield return obj;

            lastRow = newRow;
        }

        private int getColumn(float position) => MathHelper.Clamp((int)Math.Floor(position / localXDivisor), 0, availableColumns - 1);
    }
}
