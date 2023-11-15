// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Taiko.Objects
{
    /// <summary>
    /// Base class for taiko hitobjects that can become strong (large).
    /// </summary>
    public abstract class TaikoStrongableHitObject : TaikoHitObject
    {
        /// <summary>
        /// Scale multiplier for a strong drawable taiko hit object.
        /// </summary>
        public const float STRONG_SCALE = 1 / 0.65f;

        /// <summary>
        /// Default size of a strong drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_STRONG_SIZE = DEFAULT_SIZE * STRONG_SCALE;

        public readonly Bindable<bool> IsStrongBindable = new BindableBool();

        /// <summary>
        /// Whether this HitObject is a "strong" type.
        /// Strong hit objects give more points for hitting the hit object with both keys.
        /// </summary>
        public bool IsStrong
        {
            get => IsStrongBindable.Value;
            set => IsStrongBindable.Value = value;
        }

        protected TaikoStrongableHitObject()
        {
            IsStrongBindable.BindValueChanged(_ => updateSamplesFromType());
            SamplesBindable.BindCollectionChanged((_, _) => updateTypeFromSamples());
        }

        private void updateTypeFromSamples()
        {
            IsStrong = getStrongSamples().Any();
        }

        private void updateSamplesFromType()
        {
            var strongSamples = getStrongSamples();

            if (IsStrongBindable.Value != strongSamples.Any())
            {
                if (IsStrongBindable.Value)
                    Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_FINISH));
                else
                {
                    foreach (var sample in strongSamples)
                        Samples.Remove(sample);
                }
            }
        }

        private HitSampleInfo[] getStrongSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_FINISH).ToArray();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            if (IsStrong)
                AddNested(CreateStrongNestedHit(this.GetEndTime()));
        }

        /// <summary>
        /// Creates a <see cref="StrongNestedHitObject"/> representing a second hit on this object.
        /// This is only called if <see cref="IsStrong"/> is true.
        /// </summary>
        /// <param name="startTime">The start time of the nested hit.</param>
        protected abstract StrongNestedHitObject CreateStrongNestedHit(double startTime);

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not TaikoStrongableHitObject taikoStrongableHitObject)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(TaikoStrongableHitObject)}");

            IsStrong = taikoStrongableHitObject.IsStrong;
        }
    }
}
