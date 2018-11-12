// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasPosition
    {
        public const double OBJECT_RADIUS = 64;

        public event Action<Vector2> PositionChanged;
        public event Action<int> StackHeightChanged;
        public event Action<float> ScaleChanged;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        private Vector2 position;

        public virtual Vector2 Position
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

        private int stackHeight;

        public int StackHeight
        {
            get => stackHeight;
            set
            {
                if (stackHeight == value)
                    return;
                stackHeight = value;

                StackHeightChanged?.Invoke(value);
            }
        }

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
