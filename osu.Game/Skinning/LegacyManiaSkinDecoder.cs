// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;

namespace osu.Game.Skinning
{
    public class LegacyManiaSkinDecoder : LegacyDecoder<List<LegacyManiaSkinConfiguration>>
    {
        public LegacyManiaSkinDecoder()
            : base(1)
        {
        }

        private readonly List<string> pendingLines = new List<string>();
        private LegacyManiaSkinConfiguration currentConfig;

        protected override void OnBeginNewSection(Section section)
        {
            base.OnBeginNewSection(section);

            // If a new section is reached with pending lines remaining, they can all be discarded as there isn't a valid configuration to parse them into.
            pendingLines.Clear();
            currentConfig = null;
        }

        protected override void ParseLine(List<LegacyManiaSkinConfiguration> output, Section section, ReadOnlySpan<char> line)
        {
            switch (section)
            {
                case Section.Mania:
                    var pair = SplitKeyVal(line);

                    switch (pair.Key)
                    {
                        case "Keys":
                            currentConfig = new LegacyManiaSkinConfiguration(Parsing.ParseInt(pair.Value));

                            // Silently ignore duplicate configurations.
                            if (output.All(c => c.Keys != currentConfig.Keys))
                                output.Add(currentConfig);

                            // All existing lines can be flushed now that we have a valid configuration.
                            flushPendingLines();
                            break;

                        default:
                            pendingLines.Add(line.ToString());

                            // Hold all lines until a "Keys" item is found.
                            if (currentConfig != null)
                                flushPendingLines();
                            break;
                    }

                    break;
            }
        }

        private void flushPendingLines()
        {
            Debug.Assert(currentConfig != null);

            foreach (string line in pendingLines)
            {
                var pair = SplitKeyVal(line.AsSpan());

                switch (pair.Key)
                {
                    case "ColumnLineWidth":
                        parseArrayValue(pair.Value, currentConfig.ColumnLineWidth, false);
                        break;

                    case "ColumnSpacing":
                        parseArrayValue(pair.Value, currentConfig.ColumnSpacing);
                        break;

                    case "ColumnWidth":
                        parseArrayValue(pair.Value, currentConfig.ColumnWidth);
                        break;

                    case "HitPosition":
                        currentConfig.HitPosition = (480 - Math.Clamp(Parsing.ParseFloat(pair.Value), 240, 480)) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "LightPosition":
                        currentConfig.LightPosition = (480 - Parsing.ParseFloat(pair.Value)) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "ScorePosition":
                        currentConfig.ScorePosition = Parsing.ParseFloat(pair.Value) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "JudgementLine":
                        currentConfig.ShowJudgementLine = pair.Value.SequenceEqual("1");
                        break;

                    case "KeysUnderNotes":
                        currentConfig.KeysUnderNotes = pair.Value.SequenceEqual("1");
                        break;

                    case "LightingNWidth":
                        parseArrayValue(pair.Value, currentConfig.ExplosionWidth);
                        break;

                    case "LightingLWidth":
                        parseArrayValue(pair.Value, currentConfig.HoldNoteLightWidth);
                        break;

                    case "NoteBodyStyle":
                        if (Enum.TryParse<LegacyNoteBodyStyle>(pair.Value, out var style))
                            currentConfig.NoteBodyStyle = style;
                        break;

                    case "WidthForNoteHeightScale":
                        currentConfig.WidthForNoteHeightScale = Parsing.ParseFloat(pair.Value) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case { } when pair.Key.StartsWith("Colour", StringComparison.Ordinal):
                        HandleColours(currentConfig, line, true);
                        break;

                    // Custom sprite paths
                    case { } when pair.Key.StartsWith("NoteImage", StringComparison.Ordinal):
                    case { } when pair.Key.StartsWith("KeyImage", StringComparison.Ordinal):
                    case { } when pair.Key.StartsWith("Hit", StringComparison.Ordinal):
                    case { } when pair.Key.StartsWith("Stage", StringComparison.Ordinal):
                    case { } when pair.Key.StartsWith("Lighting", StringComparison.Ordinal):
                        currentConfig.ImageLookups[pair.Key.ToString()] = pair.Value.ToString();
                        break;
                }
            }

            pendingLines.Clear();
        }

        private void parseArrayValue(ReadOnlySpan<char> value, float[] output, bool applyScaleFactor = true)
        {
            int i = 0;

            foreach (var v in value.Split(','))
            {
                if (i >= output.Length)
                    break;

                output[i] = Parsing.ParseFloat(v) * (applyScaleFactor ? LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR : 1);
                i++;
            }
        }
    }
}
