// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using osu.Game.Beatmaps.Formats;

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

        protected override void ParseLine(List<LegacyManiaSkinConfiguration> output, Section section, string line)
        {
            switch (section)
            {
                case Section.Mania:
                    var pair = SplitKeyVal(line);

                    switch (pair.Key)
                    {
                        case "Keys":
                            currentConfig = new LegacyManiaSkinConfiguration(int.Parse(pair.Value, CultureInfo.InvariantCulture));

                            // Silently ignore duplicate configurations.
                            if (output.All(c => c.Keys != currentConfig.Keys))
                                output.Add(currentConfig);

                            // All existing lines can be flushed now that we have a valid configuration.
                            flushPendingLines();
                            break;

                        default:
                            pendingLines.Add(line);

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

            foreach (var line in pendingLines)
            {
                var pair = SplitKeyVal(line);

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
                        currentConfig.HitPosition = (480 - Math.Clamp(float.Parse(pair.Value, CultureInfo.InvariantCulture), 240, 480)) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "LightPosition":
                        currentConfig.LightPosition = (480 - float.Parse(pair.Value, CultureInfo.InvariantCulture)) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "ScorePosition":
                        currentConfig.ScorePosition = (float.Parse(pair.Value, CultureInfo.InvariantCulture)) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        break;

                    case "JudgementLine":
                        currentConfig.ShowJudgementLine = pair.Value == "1";
                        break;

                    case "KeysUnderNotes":
                        currentConfig.KeysUnderNotes = pair.Value == "1";
                        break;

                    case "LightingNWidth":
                        parseArrayValue(pair.Value, currentConfig.ExplosionWidth);
                        break;

                    case "LightingLWidth":
                        parseArrayValue(pair.Value, currentConfig.HoldNoteLightWidth);
                        break;

                    case "WidthForNoteHeightScale":
                        float minWidth = float.Parse(pair.Value, CultureInfo.InvariantCulture) * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
                        if (minWidth > 0)
                            currentConfig.MinimumColumnWidth = minWidth;
                        break;

                    case string _ when pair.Key.StartsWith("Colour", StringComparison.Ordinal):
                        HandleColours(currentConfig, line);
                        break;

                    // Custom sprite paths
                    case string _ when pair.Key.StartsWith("NoteImage", StringComparison.Ordinal):
                    case string _ when pair.Key.StartsWith("KeyImage", StringComparison.Ordinal):
                    case string _ when pair.Key.StartsWith("Hit", StringComparison.Ordinal):
                    case string _ when pair.Key.StartsWith("Stage", StringComparison.Ordinal):
                    case string _ when pair.Key.StartsWith("Lighting", StringComparison.Ordinal):
                        currentConfig.ImageLookups[pair.Key] = pair.Value;
                        break;
                }
            }

            pendingLines.Clear();
        }

        private void parseArrayValue(string value, float[] output, bool applyScaleFactor = true)
        {
            string[] values = value.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                if (i >= output.Length)
                    break;

                output[i] = float.Parse(values[i], CultureInfo.InvariantCulture) * (applyScaleFactor ? LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR : 1);
            }
        }
    }
}
