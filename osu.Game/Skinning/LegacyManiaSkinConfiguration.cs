// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinConfiguration : IHasCustomColours
    {
        public readonly int Keys;

        public Dictionary<string, Color4> CustomColours { get; set; } = new Dictionary<string, Color4>();

        public readonly float[] ColumnLineWidth;
        public readonly float[] ColumnSpacing;
        public readonly float[] ColumnWidth;

        public float HitPosition = 124.8f; // (480 - 402) * 1.6f
        public float LightPosition = 107.2f; // (480 - 413) * 1.6f
        public bool ShowJudgementLine = true;

        public LegacyManiaSkinConfiguration(int keys)
        {
            Keys = keys;

            ColumnLineWidth = new float[keys + 1];
            ColumnSpacing = new float[keys - 1];
            ColumnWidth = new float[keys];

            ColumnLineWidth.AsSpan().Fill(2);
            ColumnWidth.AsSpan().Fill(48);
        }
    }
}
