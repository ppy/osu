// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Rulesets.Objects
{
    public readonly ref struct LinearApproximator
    {
        private readonly ReadOnlySpan<Vector2> controlPoints;

        public LinearApproximator(ReadOnlySpan<Vector2> controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        public List<Vector2> CreateLinear()
        {
            var result = new List<Vector2>(controlPoints.Length);

            foreach (var c in controlPoints)
                result.Add(c);

            return result;
        }
    }
}
