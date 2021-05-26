// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchHitObject : HitObject, IHasXPosition, IHasComboInformation
    {
        public const float OBJECT_RADIUS = 64;

        public readonly Bindable<float> OriginalXBindable = new Bindable<float>();

        /// <summary>
        /// The horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        public float X
        {
            set => OriginalXBindable.Value = value;
        }

        float IHasXPosition.X => OriginalXBindable.Value;

        public readonly Bindable<float> XOffsetBindable = new Bindable<float>();

        /// <summary>
        /// A random offset applied to the horizontal position, set by the beatmap processing.
        /// </summary>
        public float XOffset
        {
            set => XOffsetBindable.Value = value;
        }

        /// <summary>
        /// The horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        /// <remarks>
        /// This value is the original <see cref="X"/> value specified in the beatmap, not affected by the beatmap processing.
        /// Use <see cref="EffectiveX"/> for a gameplay.
        /// </remarks>
        public float OriginalX => OriginalXBindable.Value;

        /// <summary>
        /// The effective horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        /// <remarks>
        /// This value is the original <see cref="X"/> value plus the offset applied by the beatmap processing.
        /// Use <see cref="OriginalX"/> if a value not affected by the offset is desired.
        /// </remarks>
        public float EffectiveX => OriginalXBindable.Value + XOffsetBindable.Value;

        public double TimePreempt = 1000;

        public readonly Bindable<int> IndexInBeatmapBindable = new Bindable<int>();

        public int IndexInBeatmap
        {
            get => IndexInBeatmapBindable.Value;
            set => IndexInBeatmapBindable.Value = value;
        }

        public virtual bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        public Bindable<int> IndexInCurrentComboBindable { get; } = new Bindable<int>();

        public int IndexInCurrentCombo
        {
            get => IndexInCurrentComboBindable.Value;
            set => IndexInCurrentComboBindable.Value = value;
        }

        public Bindable<int> ComboIndexBindable { get; } = new Bindable<int>();

        public int ComboIndex
        {
            get => ComboIndexBindable.Value;
            set => ComboIndexBindable.Value = value;
        }

        public Bindable<bool> LastInComboBindable { get; } = new Bindable<bool>();

        /// <summary>
        /// The next fruit starts a new combo. Used for explodey.
        /// </summary>
        public virtual bool LastInCombo
        {
            get => LastInComboBindable.Value;
            set => LastInComboBindable.Value = value;
        }

        public readonly Bindable<float> ScaleBindable = new Bindable<float>(1);

        public float Scale
        {
            get => ScaleBindable.Value;
            set => ScaleBindable.Value = value;
        }

        /// <summary>
        /// The seed value used for visual randomness such as fruit rotation.
        /// The value is <see cref="HitObject.StartTime"/> truncated to an integer.
        /// </summary>
        public int RandomSeed => (int)StartTime;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
