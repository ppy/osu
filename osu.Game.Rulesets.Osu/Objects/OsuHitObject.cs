// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasCombo, IHasPosition
    {
        public const double OBJECT_RADIUS = 64;

        private const double hittable_range = 300;
        public double HitWindow50 = 150;
        public double HitWindow100 = 80;
        public double HitWindow300 = 30;

        public float TimePreempt = 600;
        public float TimeFadein = 400;

        public Vector2 Position { get; set; }
        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public virtual int StackHeight { get; set; }

        public Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        public float Scale { get; set; } = 1;

        public Color4 ComboColour { get; set; } = Color4.Gray;
        public virtual bool NewCombo { get; set; }
        public int IndexInCurrentCombo { get; set; }

        public double HitWindowFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return hittable_range;
                case HitResult.Meh:
                    return HitWindow50;
                case HitResult.Good:
                    return HitWindow100;
                case HitResult.Great:
                    return HitWindow300;
            }
        }

        public HitResult ScoreResultForOffset(double offset)
        {
            if (offset < HitWindowFor(HitResult.Great))
                return HitResult.Great;
            if (offset < HitWindowFor(HitResult.Good))
                return HitResult.Good;
            if (offset < HitWindowFor(HitResult.Meh))
                return HitResult.Meh;
            return HitResult.Miss;
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);
            TimeFadein = (float)BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1200, 800, 300);

            HitWindow50 = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 200, 150, 100);
            HitWindow100 = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 140, 100, 60);
            HitWindow300 = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 80, 50, 20);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }
    }
}
