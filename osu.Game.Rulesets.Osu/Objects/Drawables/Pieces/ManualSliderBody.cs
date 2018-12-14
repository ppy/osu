// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    /// <summary>
    /// A <see cref="SliderBody"/> with the ability to set the drawn vertices manually.
    /// </summary>
    public class ManualSliderBody : SliderBody
    {
        public new void SetVertices(IReadOnlyList<Vector2> vertices)
        {
            base.SetVertices(vertices);
            Size = Path.Size;
        }
    }
}
