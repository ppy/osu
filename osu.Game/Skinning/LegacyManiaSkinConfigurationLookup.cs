// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    /// <summary>
    /// This class exists for the explicit purpose of ferrying information from ManiaBeatmap in a way LegacySkin can use it.
    /// This is because half of the mania legacy skin implementation is in LegacySkin (osu.Game project) which doesn't have visibility
    /// over ManiaBeatmap / StageDefinition.
    /// </summary>
    public class LegacyManiaSkinConfigurationLookup
    {
        /// <summary>
        /// Total columns across all stages.
        /// </summary>
        public readonly int TotalColumns;

        /// <summary>
        /// The column which is being looked up.
        /// May be null if the configuration does not apply to a specific column.
        /// Note that this is the absolute index across all stages.
        /// </summary>
        public readonly int? ColumnIndex;

        public readonly LegacyManiaSkinConfigurationLookups Lookup;

        public LegacyManiaSkinConfigurationLookup(int totalColumns, LegacyManiaSkinConfigurationLookups lookup, int? columnIndex = null)
        {
            TotalColumns = totalColumns;
            Lookup = lookup;
            ColumnIndex = columnIndex;
        }

        public override string ToString() => $"[{nameof(LegacyManiaSkinConfigurationLookup)} lookup:{Lookup} col:{ColumnIndex} totalcols:{TotalColumns}]";
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
        StagePaddingTop,
        StagePaddingBottom,
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
        WidthForNoteHeightScale,
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
        NoteBodyStyle,
        LightFramePerSecond
    }
}
