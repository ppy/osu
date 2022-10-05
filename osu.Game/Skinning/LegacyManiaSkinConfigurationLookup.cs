// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinConfigurationLookup
    {
        /// <summary>
        /// Total columns across all stages.
        /// </summary>
        public readonly int TotalColumns;

        public readonly LegacyManiaSkinConfigurationLookups Lookup;
        public readonly int? TargetColumn;

        public LegacyManiaSkinConfigurationLookup(int totalColumns, LegacyManiaSkinConfigurationLookups lookup, int? targetColumn = null)
        {
            TotalColumns = totalColumns;
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
        ScorePosition,
        LightPosition,
        HitTargetImage,
        ShowJudgementLine,
        KeyImage,
        KeyImageDown,
        NoteImage,
        HoldNoteHeadImage,
        HoldNoteTailImage,
        HoldNoteBodyImage,
        HoldNoteLightImage,
        HoldNoteLightScale,
        ExplosionImage,
        ExplosionScale,
        ColumnLineColour,
        JudgementLineColour,
        ColumnBackgroundColour,
        ColumnLightColour,
        MinimumColumnWidth,
        LeftStageImage,
        RightStageImage,
        BottomStageImage,
        Hit300g,
        Hit300,
        Hit200,
        Hit100,
        Hit50,
        Hit0,
        KeysUnderNotes,
    }
}
