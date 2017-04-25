// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a curve.
    /// </summary>
    public interface IHasCurve : IHasDistance, IHasRepeats
    {
        /// <summary>
        /// The control points that shape the curve.
        /// </summary>
        List<Vector2> ControlPoints { get; }

        /// <summary>
        /// The type of curve.
        /// </summary>
        CurveType CurveType { get; }

        /// <summary>
        /// Computes the position on the curve at a given progress, accounting for repeat logic.
        /// <para>
        /// Ranges from [0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.
        /// </para>
        /// </summary>
        /// <param name="progress">[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</param>
        Vector2 PositionAt(double progress);

        /// <summary>
        /// Finds the progress along the curve, accounting for repeat logic.
        /// </summary>
        /// <param name="progress">[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</param>
        /// <returns>[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</returns>
        double ProgressAt(double progress);

        /// <summary>
        /// Determines which repeat of the curve the progress point is on.
        /// </summary>
        /// <param name="progress">[0, 1] where 0 is the beginning of the curve and 1 is the end of the curve.</param>
        /// <returns>[0, RepeatCount] where 0 is the first run.</returns>
        int RepeatAt(double progress);
    }
}
