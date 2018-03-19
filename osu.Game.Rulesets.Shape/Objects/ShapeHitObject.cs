using osu.Game.Rulesets.Objects;
using OpenTK;
using osu.Game.Rulesets.Shape.Objects.Drawables;
using OpenTK.Graphics;
using osu.Game.Database;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Shape.Objects
{
    public abstract class ShapeHitObject : HitObject
    {
        public const double HitboxSize = 8;

        public float BPM;

        public Vector2 Position { get; set; }

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        public virtual int StackHeight { get; set; }

        public Vector2 StackOffset => new Vector2(0,0);

        public double Radius => HitboxSize * Scale;

        public float Scale { get; set; } = 1;

        public abstract HitObjectType Type { get; }

        public Color4 ComboColour { get; set; }
        public virtual bool NewCombo { get; set; }
        public int ComboIndex { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }
    }
}
