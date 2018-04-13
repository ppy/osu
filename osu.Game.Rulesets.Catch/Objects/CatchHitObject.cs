﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public abstract class CatchHitObject : HitObject, IHasXPosition, IHasComboInformation
    {
        public const double OBJECT_RADIUS = 44;

        public float X { get; set; }

        public int IndexInBeatmap { get; set; }

        public virtual FruitVisualRepresentation VisualRepresentation => (FruitVisualRepresentation)(IndexInBeatmap % 4);

        public virtual bool NewCombo { get; set; }

        public int IndexInCurrentCombo { get; set; }

        public int ComboIndex { get; set; }

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
        Pear,
        Grape,
        Raspberry,
        Pineapple,
        Banana // banananananannaanana
    }
}
