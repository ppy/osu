// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    /// <summary>
    /// Stores metrics related to a pair of osu! objects.
    /// Used heavily in the process of movement extraction.
    /// </summary>
    internal struct OsuObjectPair
    {
        /// <summary>
        /// The vector leading from the first to the second object.
        /// Scaled down by the width of the objects.
        /// </summary>
        /// TODO: consider System.Numerics.Vector2
        public Vector<double> RelativeVector { get; }

        /// <summary>
        /// The distance between the objects.
        /// Scaled down by the width of the objects.
        /// </summary>
        public double RelativeLength { get; }

        /// <summary>
        /// The time difference between the objects' start times in seconds, accounting for mod speed-up/slow-down.
        /// </summary>
        public double TimeDelta { get; }

        public OsuObjectPair([NotNull] OsuHitObject first, [NotNull] OsuHitObject second, double gameplayRate)
        {
            var firstPosition = Vector<double>.Build.Dense(new double[] { first.StackedPosition.X, first.StackedPosition.Y });
            var secondPosition = Vector<double>.Build.Dense(new double[] { second.StackedPosition.X, second.StackedPosition.Y });

            RelativeVector = (secondPosition - firstPosition) / (2 * second.Radius);

            RelativeLength = RelativeVector.L2Norm();

            TimeDelta = (second.StartTime - first.StartTime) / gameplayRate / 1000.0;
        }

        public static OsuObjectPair? Nullable([CanBeNull] OsuHitObject first, [CanBeNull] OsuHitObject second, double gameplayRate)
        {
            if (first == null || second == null)
                return null;

            return new OsuObjectPair(first, second, gameplayRate);
        }
    }
}
