// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyHoldNoteHeadPiece : LegacyNotePiece
    {
        protected override Drawable? GetAnimation(ISkinSource skin)
        {
            // TODO: Should fallback to the head from default legacy skin instead of note.
            return GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage)
                   ?? GetAnimationFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);
        }
    }
}
