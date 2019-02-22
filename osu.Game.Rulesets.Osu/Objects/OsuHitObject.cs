// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osuTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasPosition
    {
        public const double OBJECT_RADIUS = 64;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        public readonly Bindable<Vector2> PositionBindable = new Bindable<Vector2>();

        public virtual Vector2 Position
        {
            get => PositionBindable.Value;
            set => PositionBindable.Value = value;
        }

        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public readonly Bindable<int> StackHeightBindable = new Bindable<int>();

        public int StackHeight
        {
            get => StackHeightBindable.Value;
            set => StackHeightBindable.Value = value;
        }

        public Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        public readonly Bindable<float> ScaleBindable = new Bindable<float>(1);

        public float Scale
        {
            get => ScaleBindable.Value;
            set => ScaleBindable.Value = value;
        }

        public virtual bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        public virtual int IndexInCurrentCombo { get; set; }

        public virtual int ComboIndex { get; set; }

        public bool LastInCombo { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);
            TimeFadeIn = (float)BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1200, 800, 300);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }

        protected override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
