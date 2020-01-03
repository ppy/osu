// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class OddColumn : DefaultColumn
    {
        public OddColumn(int index)
            : base(index)
        {
            AccentColour = new Color4(94, 0, 57, 255);
        }
    }
}
