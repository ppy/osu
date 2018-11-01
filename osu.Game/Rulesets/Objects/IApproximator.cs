// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public interface IApproximator
    {
        /// <summary>
        /// Approximates a path by interpolating a sequence of control points.
        /// </summary>
        /// <param name="controlPoints">The control points of the path.</param>
        /// <returns>A set of points that lie on the path.</returns>
        List<Vector2> Approximate(ReadOnlySpan<Vector2> controlPoints);
    }
}
