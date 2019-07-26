// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraph : SquareGraph
    {
        public IEnumerable<double> Strains
        {
            set => Values = value.ToList();
        }
    }
}
