// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class SpinnerSpmCalculator : Component
    {
        private readonly Queue<RotationRecord> records = new Queue<RotationRecord>();
        private const double spm_count_duration = 595; // not using hundreds to avoid frame rounding issues

        /// <summary>
        /// The resultant spins per minute value, which is updated via <see cref="SetRotation"/>.
        /// </summary>
        public IBindable<double> Result => result;

        private readonly Bindable<double> result = new BindableDouble();

        [Resolved]
        private DrawableHitObject drawableSpinner { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            drawableSpinner.HitObjectApplied += resetState;
        }

        public void SetRotation(float currentRotation)
        {
            // Never calculate SPM by same time of record to avoid 0 / 0 = NaN or X / 0 = Infinity result.
            if (Precision.AlmostEquals(0, Time.Elapsed))
                return;

            // If we've gone back in time, it's fine to work with a fresh set of records for now
            if (records.Count > 0 && Time.Current < records.Last().Time)
                records.Clear();

            if (records.Count > 0)
            {
                var record = records.Peek();
                while (records.Count > 0 && Time.Current - records.Peek().Time > spm_count_duration)
                    record = records.Dequeue();

                result.Value = (currentRotation - record.Rotation) / (Time.Current - record.Time) * 1000 * 60 / 360;
            }

            records.Enqueue(new RotationRecord { Rotation = currentRotation, Time = Time.Current });
        }

        private void resetState(DrawableHitObject hitObject)
        {
            result.Value = 0;
            records.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner != null)
                drawableSpinner.HitObjectApplied -= resetState;
        }

        private struct RotationRecord
        {
            public float Rotation;
            public double Time;
        }
    }
}
