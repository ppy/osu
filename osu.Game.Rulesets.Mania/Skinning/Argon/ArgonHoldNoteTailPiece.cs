// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonHoldNoteTailPiece : CompositeDrawable
    {
        public ArgonHoldNoteTailPiece()
        {
            // holds end at the middle of the tail,
            // so we do * 2 pull up the hold body to be the height of a note
            Height = ArgonNotePiece.NOTE_HEIGHT * 2;
        }
    }
}
