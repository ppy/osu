// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonHoldNoteHeadPiece : ArgonNotePiece
    {
        protected override Drawable CreateIcon() => new Circle
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Y = 2,
            Size = new Vector2(20, 5),
        };
    }
}
