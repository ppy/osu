// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class SpecialColumn : DefaultColumn
    {
        public SpecialColumn(int index) : base(index)
        {
            Width = 70;
            AccentColour = new Color4(0, 48, 63, 255);
        }
    }
}
