// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public class LegacySkinDecoder : LegacyDecoder<SkinConfiguration>
    {
        public LegacySkinDecoder()
            : base(1)
        {
        }

        protected override void ParseLine(SkinConfiguration skin, Section section, ReadOnlySpan<char> line)
        {
            if (section != Section.Colours)
            {
                var pair = SplitKeyVal(line);

                switch (section)
                {
                    case Section.General:
                        switch (pair.Key)
                        {
                            case @"Name":
                                skin.SkinInfo.Name = pair.Value.ToString();
                                return;

                            case @"Author":
                                skin.SkinInfo.Creator = pair.Value.ToString();
                                return;

                            case @"Version":
                                if (pair.Value == "latest")
                                    skin.LegacyVersion = SkinConfiguration.LATEST_VERSION;
                                else if (decimal.TryParse(pair.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal version))
                                    skin.LegacyVersion = version;

                                return;
                        }

                        break;

                    // osu!catch section only has colour settings
                    // so no harm in handling the entire section
                    case Section.CatchTheBeat:
                        HandleColours(skin, line, true);
                        return;
                }

                if (!pair.Key.IsEmpty)
                    skin.ConfigDictionary[pair.Key.ToString()] = pair.Value.ToString();
            }

            base.ParseLine(skin, section, line);
        }

        protected override SkinConfiguration CreateTemplateObject()
        {
            var config = base.CreateTemplateObject();
            config.LegacyVersion = 1.0m;
            return config;
        }
    }
}
