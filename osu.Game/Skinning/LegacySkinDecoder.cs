// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public class LegacySkinDecoder : LegacyDecoder<DefaultSkinConfiguration>
    {
        public LegacySkinDecoder()
            : base(1)
        {
        }

        protected override void ParseSectionLine(string line)
        {
            if (ConfigSection != Section.Colours)
            {
                line = StripComments(line);

                var pair = SplitKeyVal(line);

                switch (ConfigSection)
                {
                    case Section.General:
                        switch (pair.Key)
                        {
                            case @"Name":
                                Output.SkinInfo.Name = pair.Value;
                                return;

                            case @"Author":
                                Output.SkinInfo.Creator = pair.Value;
                                return;
                        }

                        break;
                }

                if (!string.IsNullOrEmpty(pair.Key))
                    Output.ConfigDictionary[pair.Key] = pair.Value;
            }

            base.ParseSectionLine(line);
        }
    }
}
