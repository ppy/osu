// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchHitObject : HitObject, IHasXPosition, IHasComboInformation
    {
        public const float OBJECT_RADIUS = 64;

        // This value is after XOffset applied.
        public readonly Bindable<float> XBindable = new Bindable<float>();

        // This value is before XOffset applied.
        private float originalX;

        /// <summary>
        /// The horizontal position of the fruit between 0 and <see cref="CatchPlayfield.WIDTH"/>.
        /// </summary>
        public float X
        {
            // TODO: I don't like this asymmetry.
            get => XBindable.Value;
            // originalX is set by `XBindable.BindValueChanged`
            set => XBindable.Value = value + xOffset;
        }

        private float xOffset;

        /// <summary>
        /// A random offset applied to <see cref="X"/>, set by the <see cref="CatchBeatmapProcessor"/>.
        /// </summary>
        internal float XOffset
        {
            get => xOffset;
            set
            {
                xOffset = value;
                XBindable.Value = originalX + xOffset;
            }
        }

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

        protected CatchHitObject()
        {
            XBindable.BindValueChanged(x => originalX = x.NewValue - xOffset);
        }
    }
}
