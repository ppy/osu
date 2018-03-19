using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Rulesets.Shape.Objects
{
    public class BaseShape : ShapeHitObject
    {
        public override HitObjectType Type => HitObjectType.Shape;
        public Vector2 StartPosition { get; set; }
        public float ShapeSize { get; set; } = 40;
        public int ShapeID { get; set; }

        /// <summary>
        /// The hit window that results in a "GREAT" hit.
        /// </summary>
        public double HitWindowGreat = 35;

        /// <summary>
        /// The hit window that results in a "GOOD" hit.
        /// </summary>
        public double HitWindowGood = 80;

        /// <summary>
        /// The hit window that results in a "MISS".
        /// </summary>
        public double HitWindowMiss = 95;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            HitWindowGreat = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 50, 35, 20);
            HitWindowGood = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 120, 80, 50);
            HitWindowMiss = BeatmapDifficulty.DifficultyRange(difficulty.OverallDifficulty, 135, 95, 70);
        }
    }
}
