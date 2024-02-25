// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class SpinnerSpmCalculator : Component
    {
        private readonly Queue<RotationRecord> records = new Queue<RotationRecord>();
        private const double spm_count_duration = 595; // not using hundreds to avoid frame rounding issues

        /// <summary>
        /// The resultant spins per minute value, which is updated via <see cref="SetRotation"/>.
        /// </summary>
        public IBindable<double> Result => result;

        private readonly Bindable<double> result = new BindableDouble();

        [Resolved]
        private DrawableHitObject drawableSpinner { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            drawableSpinner.HitObjectApplied += resetState;
        }

        private RotationRecord lastRecord;

        public void SetRotation(float currentRotation)
        {
            // If we've gone back in time, it's fine to work with a fresh set of records for now
            if (records.Count > 0 && Time.Current < lastRecord.Time)
                records.Clear();

            // Never calculate SPM by same time of record to avoid 0 / 0 = NaN or X / 0 = Infinity result.
            if (records.Count > 0 && Precision.AlmostEquals(Time.Current, lastRecord.Time))
                return;

            if (records.Count > 0)
            {
                var record = records.Peek();
                while (records.Count > 0 && Time.Current - records.Peek().Time > spm_count_duration)
                    record = records.Dequeue();

                result.Value = (currentRotation - record.Rotation) / (Time.Current - record.Time) * 1000 * 60 / 360;
            }

            records.Enqueue(lastRecord = new RotationRecord { Rotation = currentRotation, Time = Time.Current });
        }

        private void resetState(DrawableHitObject hitObject)
        {
            lastRecord = default;
            result.Value = 0;
            records.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableSpinner.IsNotNull())
                drawableSpinner.HitObjectApplied -= resetState;
        }

        private struct RotationRecord
        {
            public float Rotation;
            public double Time;
        }
    }
}
