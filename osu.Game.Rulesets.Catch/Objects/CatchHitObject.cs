// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchHitObject : HitObject, IHasPosition, IHasComboInformation
    {
        public const float OBJECT_RADIUS = 64;

        private HitObjectProperty<float> originalX;

        public Bindable<float> OriginalXBindable => originalX.Bindable;

        /// <summary>
        /// The horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        /// <remarks>
        /// Only setter is exposed.
        /// Use <see cref="OriginalX"/> or <see cref="EffectiveX"/> to get the horizontal position.
        /// </remarks>
        [JsonIgnore]
        public float X
        {
            set => originalX.Value = value;
        }

        private HitObjectProperty<float> xOffset;

        public Bindable<float> XOffsetBindable => xOffset.Bindable;

        /// <summary>
        /// A random offset applied to the horizontal position, set by the beatmap processing.
        /// </summary>
        public float XOffset
        {
            get => xOffset.Value;
            set => xOffset.Value = value;
        }

        /// <summary>
        /// The horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        /// <remarks>
        /// This value is the original <see cref="X"/> value specified in the beatmap, not affected by the beatmap processing.
        /// Use <see cref="EffectiveX"/> for a gameplay.
        /// </remarks>
        public float OriginalX
        {
            get => originalX.Value;
            set => originalX.Value = value;
        }

        /// <summary>
        /// The effective horizontal position of the hit object between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        /// <remarks>
        /// This value is the original <see cref="X"/> value plus the offset applied by the beatmap processing.
        /// Use <see cref="OriginalX"/> if a value not affected by the offset is desired.
        /// </remarks>
        public float EffectiveX => Math.Clamp(OriginalX + XOffset, 0, CatchPlayfield.WIDTH);

        public double TimePreempt { get; set; } = 1000;

        private HitObjectProperty<int> indexInBeatmap;

        public Bindable<int> IndexInBeatmapBindable => indexInBeatmap.Bindable;

        public int IndexInBeatmap
        {
            get => indexInBeatmap.Value;
            set => indexInBeatmap.Value = value;
        }

        public virtual bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        private HitObjectProperty<int> indexInCurrentCombo;

        public Bindable<int> IndexInCurrentComboBindable => indexInCurrentCombo.Bindable;

        public int IndexInCurrentCombo
        {
            get => indexInCurrentCombo.Value;
            set => indexInCurrentCombo.Value = value;
        }

        private HitObjectProperty<int> comboIndex;

        public Bindable<int> ComboIndexBindable => comboIndex.Bindable;

        public int ComboIndex
        {
            get => comboIndex.Value;
            set => comboIndex.Value = value;
        }

        private HitObjectProperty<int> comboIndexWithOffsets;

        public Bindable<int> ComboIndexWithOffsetsBindable => comboIndexWithOffsets.Bindable;

        public int ComboIndexWithOffsets
        {
            get => comboIndexWithOffsets.Value;
            set => comboIndexWithOffsets.Value = value;
        }

        private HitObjectProperty<bool> lastInCombo;

        public Bindable<bool> LastInComboBindable => lastInCombo.Bindable;

        /// <summary>
        /// The next fruit starts a new combo. Used for explodey.
        /// </summary>
        public virtual bool LastInCombo
        {
            get => lastInCombo.Value;
            set => lastInCombo.Value = value;
        }

        private HitObjectProperty<float> scale = new HitObjectProperty<float>(1);

        public Bindable<float> ScaleBindable => scale.Bindable;

        public float Scale
        {
            get => scale.Value;
            set => scale.Value = value;
        }

        /// <summary>
        /// The seed value used for visual randomness such as fruit rotation.
        /// The value is <see cref="HitObject.StartTime"/> truncated to an integer.
        /// </summary>
        public int RandomSeed => (int)StartTime;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);

            Scale = LegacyRulesetExtensions.CalculateScaleFromCircleSize(difficulty.CircleSize);
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        #region Hit object conversion

        // The half of the height of the osu! playfield.
        public const float DEFAULT_LEGACY_CONVERT_Y = 192;

        /// <summary>
        /// The Y position of the hit object is not used in the normal osu!catch gameplay.
        /// It is preserved to maximize the backward compatibility with the legacy editor, in which the mappers use the Y position to organize the patterns.
        /// </summary>
        public float LegacyConvertedY { get; set; } = DEFAULT_LEGACY_CONVERT_Y;

        float IHasXPosition.X => OriginalX;

        float IHasYPosition.Y => LegacyConvertedY;

        Vector2 IHasPosition.Position => new Vector2(OriginalX, LegacyConvertedY);

        #endregion

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not CatchHitObject catchOther)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(CatchHitObject)}");

            XOffset = catchOther.XOffset;
            OriginalX = catchOther.OriginalX;
            TimePreempt = catchOther.TimePreempt;
            IndexInBeatmap = catchOther.IndexInBeatmap;
            NewCombo = catchOther.NewCombo;
            ComboOffset = catchOther.ComboOffset;
            IndexInCurrentCombo = catchOther.IndexInCurrentCombo;
            ComboIndex = catchOther.ComboIndex;
            ComboIndexWithOffsets = catchOther.ComboIndexWithOffsets;
            LastInCombo = catchOther.LastInCombo;
            Scale = catchOther.Scale;
            LegacyConvertedY = catchOther.LegacyConvertedY;
        }
    }
}
