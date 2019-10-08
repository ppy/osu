// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;
using System;

namespace osu.Game.Skinning
{
    public class LegacySkinColourDecoder : LegacyDecoder<DefaultColourConfiguration>
    {
        public LegacySkinColourDecoder()
            : base(1)
        {
        }

        private bool hasComboColours;

        protected override void ParseLine(DefaultColourConfiguration skin, Section section, string line)
        {
            if (section == Section.Colours)
            {
                var pair = SplitKeyVal(StripComments(line));

                string[] split = pair.Value.Split(',');

                if (split.Length != 3 && split.Length != 4)
                    throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B or R,G,B,A): {pair.Value}");

                Color4 colour;

                try
                {
                    colour = new Color4(byte.Parse(split[0]), byte.Parse(split[1]), byte.Parse(split[2]), split.Length == 4 ? byte.Parse(split[3]) : (byte)255);
                }
                catch
                {
                    throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");
                }

                if (pair.Key.StartsWith("Combo"))
                {
                    // Check if default configuration provides combo colours to clear them.
                    if (skin.ComboColours.Count > 0 && !hasComboColours)
                    {
                        skin.ComboColours.Clear();
                        hasComboColours = true;
                    }

                    skin.ComboColours.Add(colour);
                }
                else
                    skin.CustomColours[pair.Key] = colour;
            }
        }
    }
}
