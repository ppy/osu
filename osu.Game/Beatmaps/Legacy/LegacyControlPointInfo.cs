// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Legacy
{
    public class LegacyControlPointInfo : ControlPointInfo
    {
        /// <summary>
        /// All sound points.
        /// </summary>
        [JsonProperty]
        public IBindableList<SampleControlPoint> SamplePoints => samplePoints;

        private readonly BindableList<SampleControlPoint> samplePoints = new BindableList<SampleControlPoint>();

        /// <summary>
        /// Finds the sound control point that is active at <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to find the sound control point at.</param>
        /// <returns>The sound control point.</returns>
        [NotNull]
        public SampleControlPoint SamplePointAt(double time) => BinarySearchWithFallback(SamplePoints, time, SamplePoints.Count > 0 ? SamplePoints[0] : SampleControlPoint.DEFAULT);

        public override void Clear()
        {
            base.Clear();
            samplePoints.Clear();
        }

        protected override bool CheckAlreadyExisting(double time, ControlPoint newPoint)
        {
            if (newPoint is SampleControlPoint)
            {
                var existing = BinarySearch(SamplePoints, time);
                return newPoint.IsRedundant(existing);
            }

            return base.CheckAlreadyExisting(time, newPoint);
        }

        protected override void GroupItemAdded(ControlPoint controlPoint)
        {
            if (controlPoint is SampleControlPoint typed)
                samplePoints.Add(typed);

            base.GroupItemAdded(controlPoint);
        }

        protected override void GroupItemRemoved(ControlPoint controlPoint)
        {
            if (controlPoint is SampleControlPoint typed)
                samplePoints.Remove(typed);

            base.GroupItemRemoved(controlPoint);
        }
    }
}
