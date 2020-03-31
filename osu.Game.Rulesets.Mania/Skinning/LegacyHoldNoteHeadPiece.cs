// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHoldNoteHeadPiece : LegacyNotePiece
    {
        protected override Texture GetTexture(ISkinSource skin)
        {
            return GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage)
                   ?? GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.NoteImage);
        }
    }
}
