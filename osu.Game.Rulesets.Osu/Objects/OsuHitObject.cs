// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit.Types;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasEditablePosition
    {
        public const double OBJECT_RADIUS = 64;

        public event Action<Vector2> PositionChanged;
        public event Action<float> ScaleChanged;
        public event Action<int> IndexInCurrentComboChanged;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        private Vector2 position;

        public Vector2 Position
        {
            get => position;
            set
            {
                if (position == value)
                    return;
                position = value;

                PositionChanged?.Invoke(value);
            }
        }

        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public virtual int StackHeight { get; set; }

        public Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        private float scale = 1;

        public float Scale
        {
            get => scale;
            set
            {
                if (scale == value)
                    return;
                scale = value;

                ScaleChanged?.Invoke(value);
            }
        }

        public virtual bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        private int indexInCurrentCombo;

        public virtual int IndexInCurrentCombo
        {
            get => indexInCurrentCombo;
            set
            {
                if (indexInCurrentCombo == value)
                    return;
                indexInCurrentCombo = value;

                IndexInCurrentComboChanged?.Invoke(value);
            }
        }

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

        public virtual void AdjustPosition(Vector2 position) => Position = position;
    }
}
