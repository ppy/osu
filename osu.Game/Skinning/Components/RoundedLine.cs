// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Skinning.Components
{
    public partial class RoundedLine : Circle, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        public RoundedLine()
        {
            Size = new Vector2(200, 8);
        }
    }
}
