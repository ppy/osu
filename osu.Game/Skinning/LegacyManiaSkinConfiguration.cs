// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinConfiguration : IHasCustomColours
    {
        /// <summary>
        /// Conversion factor from converting legacy positioning values (based in x480 dimensions) to x768.
        /// </summary>
        public const float POSITION_SCALE_FACTOR = 1.6f;

        /// <summary>
        /// Size of a legacy column in the default skin, used for determining relative scale factors.
        /// </summary>
        public const float DEFAULT_COLUMN_SIZE = 30 * POSITION_SCALE_FACTOR;

        public const float DEFAULT_HIT_POSITION = (480 - 402) * POSITION_SCALE_FACTOR;

        public readonly int Keys;

        public Dictionary<string, Color4> CustomColours { get; } = new Dictionary<string, Color4>();

        public Dictionary<string, string> ImageLookups = new Dictionary<string, string>();

        public float WidthForNoteHeightScale;

        public readonly float[] ColumnLineWidth;
        public readonly float[] ColumnSpacing;
        public readonly float[] ColumnWidth;
        public readonly float[] ExplosionWidth;
        public readonly float[] HoldNoteLightWidth;

        public float HitPosition = DEFAULT_HIT_POSITION;
        public float LightPosition = (480 - 413) * POSITION_SCALE_FACTOR;
        public float ComboPosition = 111 * POSITION_SCALE_FACTOR;
        public float ScorePosition = 300 * POSITION_SCALE_FACTOR;
        public float BarLineHeight = 1.2f;
        public bool ShowJudgementLine = true;
        public bool KeysUnderNotes;
        public int LightFramePerSecond = 60;

        public LegacyNoteBodyStyle? NoteBodyStyle;

        #region Unimplemented properties, at this time present primarily for encode-decode stability

        public LegacySpecialStyle? SpecialStyle;
        public float ColumnStart = 136; // TODO: likely needs POSITION_SCALE_FACTOR
        public float ColumnRight = 19; // TODO: likely needs POSITION_SCALE_FACTOR
        public bool UpsideDown;
        public bool SeparateScore = true;
        public bool SplitStages;
        public float StageSeparation = 40;
        public LegacyComboBurstStyle? ComboBurstStyle;

        public Dictionary<string, string> FlipSettings { get; } = new Dictionary<string, string>();
        public LegacyNoteBodyStyle?[] ColumnNoteBodyStyles;

        #endregion

        public LegacyManiaSkinConfiguration(int keys)
        {
            Keys = keys;

            ColumnLineWidth = new float[keys + 1];
            ColumnSpacing = new float[keys - 1];
            ColumnWidth = new float[keys];
            ExplosionWidth = new float[keys];
            HoldNoteLightWidth = new float[keys];
            ColumnNoteBodyStyles = new LegacyNoteBodyStyle?[keys];

            ColumnLineWidth.AsSpan().Fill(2);
            ColumnWidth.AsSpan().Fill(DEFAULT_COLUMN_SIZE);
        }

        public float MinimumColumnWidth => ColumnWidth.Min();

        /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L321-L326"/>
        public enum LegacySpecialStyle
        {
            None = 0,
            Left = 1,
            Right = 2,
        }

        /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L328-L336"/>
        public enum LegacyNoteBodyStyle
        {
            Stretch = 0,

            // listed as the default on https://osu.ppy.sh/wiki/en/Skinning/skin.ini, but is seemingly not according to the source.
            // Repeat = 1,

            RepeatTop = 2,
            RepeatBottom = 3,
            RepeatTopAndBottom = 4,
        }

        /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/0b8b19af621dbb282773c22b36cc0453942b98d8/osu!/Graphics/Skinning/SkinMania.cs#L338-L343"/>
        public enum LegacyComboBurstStyle
        {
            Left = 0,
            Right = 1,
            Both = 2,
        }
    }
}
