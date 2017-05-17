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
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Special converter used for converting from osu!stable beatmaps.
    /// </summary>
    internal class LegacyConverter
    {
        private const int max_previous_note_times = 7;

        private readonly FastRandom random;

        private readonly List<double> previousNoteTimes;
        private readonly bool[] previousNotes;
        private readonly double lastNoteTime;
        private readonly float lastNotePosition;

        private readonly int availableColumns;
        private readonly float localXDivisor; 

        private readonly Beatmap beatmap;

        public LegacyConverter(ObjectRow previousrow, Beatmap beatmap)
        {
            this.beatmap = beatmap;

            int seed = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.DrainRate + beatmap.BeatmapInfo.Difficulty.CircleSize)
                * 20 + (int)(beatmap.BeatmapInfo.Difficulty.OverallDifficulty * 41.2) + (int)Math.Round(beatmap.BeatmapInfo.Difficulty.ApproachRate);
            random = new FastRandom(seed);

            availableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);
            localXDivisor = 512.0f / availableColumns;

            previousNoteTimes = new List<double>(max_previous_note_times);
            previousNotes = new bool[availableColumns];
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
                conversion = new DistanceObjectConversion(distanceData, beatmap);
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

            foreach (ManiaHitObject obj in conversion.GenerateConversion())
                yield return obj;
        }

        private int getColumn(float position) => MathHelper.Clamp((int)Math.Floor(position / localXDivisor), 0, availableColumns - 1);
    }
}
