// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
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
