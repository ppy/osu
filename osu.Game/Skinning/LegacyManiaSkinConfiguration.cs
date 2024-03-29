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
        public float ScorePosition = 300 * POSITION_SCALE_FACTOR;
        public bool ShowJudgementLine = true;
        public bool KeysUnderNotes;
        public int LightFramePerSecond = 60;

        public HoldNoteTailOrigin HoldNoteTailOrigin = HoldNoteTailOrigin.Bottom;
        public LegacyNoteBodyStyle? NoteBodyStyle;

        public LegacyManiaSkinConfiguration(int keys)
        {
            Keys = keys;

            ColumnLineWidth = new float[keys + 1];
            ColumnSpacing = new float[keys - 1];
            ColumnWidth = new float[keys];
            ExplosionWidth = new float[keys];
            HoldNoteLightWidth = new float[keys];

            ColumnLineWidth.AsSpan().Fill(2);
            ColumnWidth.AsSpan().Fill(DEFAULT_COLUMN_SIZE);
        }

        public float MinimumColumnWidth => ColumnWidth.Min();
    }
}
