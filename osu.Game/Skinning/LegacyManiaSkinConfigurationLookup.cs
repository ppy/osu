// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinConfigurationLookup
    {
        public readonly int Keys;
        public readonly LegacyManiaSkinConfigurationLookups Lookup;
        public readonly int? TargetColumn;

        public LegacyManiaSkinConfigurationLookup(int keys, LegacyManiaSkinConfigurationLookups lookup, int? targetColumn = null)
        {
            Keys = keys;
            Lookup = lookup;
            TargetColumn = targetColumn;
        }
    }

    public enum LegacyManiaSkinConfigurationLookups
    {
        ColumnWidth,
        ColumnSpacing,
        LightImage,
        LeftLineWidth,
        RightLineWidth,
        HitPosition,
        LightPosition,
        HitTargetImage,
        ShowJudgementLine,
        KeyImage,
        KeyImageDown,
        NoteImage,
        HoldNoteHeadImage,
        HoldNoteTailImage,
        HoldNoteBodyImage,
        ExplosionImage,
        ExplosionScale,
        ColumnLineColour,
        MinimumColumnWidth
    }
}
