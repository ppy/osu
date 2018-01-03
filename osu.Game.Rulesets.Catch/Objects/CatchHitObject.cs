// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchHitObject : HitObject, IHasXPosition, IHasCombo
    {
        public const double OBJECT_RADIUS = 44;

        public float X { get; set; }

        public Color4 ComboColour
        {
            get
            {
                switch (VisualRepresentation)
                {
                    default:
                    case FruitVisualRepresentation.Triforce:
                        return new Color4(17, 136, 170, 255);
                    case FruitVisualRepresentation.Grape:
                        return new Color4(204, 102, 0, 255);
                    case FruitVisualRepresentation.DPad:
                        return new Color4(121, 9, 13, 255);
                    case FruitVisualRepresentation.Pineapple:
                        return new Color4(102, 136, 0, 255);
                    case FruitVisualRepresentation.Banana:
                        switch (RNG.Next(0, 3))
                        {
                            default:
                                return new Color4(255, 240, 0, 255);
                            case 1:
                                return new Color4(255, 192, 0, 255);
                            case 2:
                                return new Color4(214, 221, 28, 255);
                        }
                }
            }

            set { }
        }

        public int IndexInBeatmap { get; set; }

        public virtual FruitVisualRepresentation VisualRepresentation => (FruitVisualRepresentation)(IndexInBeatmap % 4);

        public virtual bool NewCombo { get; set; }

        /// <summary>
        /// The next fruit starts a new combo. Used for explodey.
        /// </summary>
        public virtual bool LastInCombo { get; set; }

        public float Scale { get; set; } = 1;

        /// <summary>
        /// Whether this fruit can initiate a hyperdash.
        /// </summary>
        public bool HyperDash => HyperDashTarget != null;

        /// <summary>
        /// The target fruit if we are to initiate a hyperdash.
        /// </summary>
        public CatchHitObject HyperDashTarget;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            Scale = 1.0f - 0.7f * (difficulty.CircleSize - 5) / 5;
        }
    }

    public enum FruitVisualRepresentation
    {
        Triforce,
        Grape,
        DPad,
        Pineapple,
        Banana // banananananannaanana
    }
}
