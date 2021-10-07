// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Mods
{
    public class DifficultyBindable : Bindable<float?>
    {
        /// <summary>
        /// Whether the extended limits should be applied to this bindable.
        /// </summary>
        public readonly BindableBool ExtendedLimits = new BindableBool();

        /// <summary>
        /// An internal numeric bindable to hold and propagate min/max/precision.
        /// The value of this bindable should not be set.
        /// </summary>
        internal readonly BindableFloat CurrentNumber = new BindableFloat
        {
            MinValue = 0,
            MaxValue = 10,
        };

        /// <summary>
        /// A function that can extract the current value of this setting from a beatmap difficulty for display purposes.
        /// </summary>
        public Func<IBeatmapDifficultyInfo, float> ReadCurrentFromDifficulty;

        public float Precision
        {
            set => CurrentNumber.Precision = value;
        }

        public float MinValue
        {
            set => CurrentNumber.MinValue = value;
        }

        private float maxValue;

        public float MaxValue
        {
            set
            {
                if (value == maxValue)
                    return;

                maxValue = value;
                updateMaxValue();
            }
        }

        private float? extendedMaxValue;

        /// <summary>
        /// The maximum value to be used when extended limits are applied.
        /// </summary>
        public float? ExtendedMaxValue
        {
            set
            {
                if (value == extendedMaxValue)
                    return;

                extendedMaxValue = value;
                updateMaxValue();
            }
        }

        public DifficultyBindable()
            : this(null)
        {
        }

        public DifficultyBindable(float? defaultValue = null)
            : base(defaultValue)
        {
            ExtendedLimits.BindValueChanged(_ => updateMaxValue());
        }

        public override float? Value
        {
            get => base.Value;
            set
            {
                // Ensure that in the case serialisation runs in the wrong order (and limit extensions aren't applied yet) the deserialised value is still propagated.
                if (value != null)
                    CurrentNumber.MaxValue = MathF.Max(CurrentNumber.MaxValue, value.Value);

                base.Value = value;
            }
        }

        private void updateMaxValue()
        {
            CurrentNumber.MaxValue = ExtendedLimits.Value && extendedMaxValue != null ? extendedMaxValue.Value : maxValue;
        }

        public override void BindTo(Bindable<float?> them)
        {
            if (!(them is DifficultyBindable otherDifficultyBindable))
                throw new InvalidOperationException($"Cannot bind to a non-{nameof(DifficultyBindable)}.");

            ReadCurrentFromDifficulty = otherDifficultyBindable.ReadCurrentFromDifficulty;

            // the following max value copies are only safe as long as these values are effectively constants.
            MaxValue = otherDifficultyBindable.maxValue;
            ExtendedMaxValue = otherDifficultyBindable.extendedMaxValue;

            ExtendedLimits.BindTarget = otherDifficultyBindable.ExtendedLimits;

            // the actual values need to be copied after the max value constraints.
            CurrentNumber.BindTarget = otherDifficultyBindable.CurrentNumber;
            base.BindTo(them);
        }

        public override void UnbindFrom(IUnbindable them)
        {
            if (!(them is DifficultyBindable otherDifficultyBindable))
                throw new InvalidOperationException($"Cannot unbind from a non-{nameof(DifficultyBindable)}.");

            base.UnbindFrom(them);

            CurrentNumber.UnbindFrom(otherDifficultyBindable.CurrentNumber);
            ExtendedLimits.UnbindFrom(otherDifficultyBindable.ExtendedLimits);
        }

        protected override Bindable<float?> CreateInstance() => new DifficultyBindable();
    }
}
