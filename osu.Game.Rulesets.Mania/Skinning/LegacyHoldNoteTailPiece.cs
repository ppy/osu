// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyHoldNoteTailPiece : LegacyNotePiece
    {
        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            // Invert the direction
            base.OnDirectionChanged(direction.NewValue == ScrollingDirection.Up
                ? new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Down, ScrollingDirection.Down)
                : new ValueChangedEvent<ScrollingDirection>(ScrollingDirection.Up, ScrollingDirection.Up));
        }

        protected override Texture GetTexture(ISkinSource skin)
        {
            return GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteTailImage)
                   ?? GetTextureFromLookup(skin, LegacyManiaSkinConfigurationLookups.HoldNoteHeadImage);
        }
    }
}
