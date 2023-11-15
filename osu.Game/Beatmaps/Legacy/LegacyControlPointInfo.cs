// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Lists;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Legacy
{
    public class LegacyControlPointInfo : ControlPointInfo
    {
        /// <summary>
        /// All sound points.
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<SampleControlPoint> SamplePoints => samplePoints;

        private readonly SortedList<SampleControlPoint> samplePoints = new SortedList<SampleControlPoint>(Comparer<SampleControlPoint>.Default);

        /// <summary>
        /// Finds the sound control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the sound control point at.</param>
        /// <returns>The sound control point.</returns>
        public SampleControlPoint SamplePointAt(double time) => BinarySearchWithFallback(SamplePoints, time, SamplePoints.Count > 0 ? SamplePoints[0] : SampleControlPoint.DEFAULT);

        /// <summary>
        /// All difficulty points.
        /// </summary>
        [JsonProperty]
        public IReadOnlyList<DifficultyControlPoint> DifficultyPoints => difficultyPoints;

        private readonly SortedList<DifficultyControlPoint> difficultyPoints = new SortedList<DifficultyControlPoint>(Comparer<DifficultyControlPoint>.Default);

        /// <summary>
        /// Finds the difficulty control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the difficulty control point at.</param>
        /// <returns>The difficulty control point.</returns>
        public DifficultyControlPoint DifficultyPointAt(double time) => BinarySearchWithFallback(DifficultyPoints, time, DifficultyControlPoint.DEFAULT);

        public override void Clear()
        {
            base.Clear();
            samplePoints.Clear();
            difficultyPoints.Clear();
        }

        protected override bool CheckAlreadyExisting(double time, ControlPoint newPoint)
        {
            switch (newPoint)
            {
                case SampleControlPoint:
                    // intentionally don't use SamplePointAt (we always need to consider the first sample point).
                    var existing = BinarySearch(SamplePoints, time);
                    return newPoint.IsRedundant(existing);

                case DifficultyControlPoint:
                    return newPoint.IsRedundant(DifficultyPointAt(time));

                default:
                    return base.CheckAlreadyExisting(time, newPoint);
            }
        }

        protected override void GroupItemAdded(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case SampleControlPoint typed:
                    samplePoints.Add(typed);
                    return;

                case DifficultyControlPoint typed:
                    difficultyPoints.Add(typed);
                    return;

                default:
                    base.GroupItemAdded(controlPoint);
                    break;
            }
        }

        protected override void GroupItemRemoved(ControlPoint controlPoint)
        {
            switch (controlPoint)
            {
                case SampleControlPoint typed:
                    samplePoints.Remove(typed);
                    break;

                case DifficultyControlPoint typed:
                    difficultyPoints.Remove(typed);
                    break;
            }

            base.GroupItemRemoved(controlPoint);
        }

        protected override ControlPointInfo CreateInstance() => new LegacyControlPointInfo();
    }
}
