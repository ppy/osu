// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Formats;

namespace osu.Game.Skinning
{
    public class LegacySkinDecoder : LegacyDecoder<SkinConfiguration>
    {
        public LegacySkinDecoder()
            : base(1)
        {
        }

        protected override void ParseLine(SkinConfiguration skin, Section section, string line)
        {
            line = StripComments(line);

            switch (section)
            {
                case Section.General:
                {
                    var pair = SplitKeyVal(line);

                    switch (pair.Key)
                    {
                        case @"Name":
                            skin.SkinInfo.Name = pair.Value;
                            break;
                        case @"Author":
                            skin.SkinInfo.Creator = pair.Value;
                            break;
                        case @"CursorExpand":
                            skin.CursorExpand = pair.Value != "0";
                            break;
                    }

                    break;
                }
                case Section.Fonts:
                {
                    var pair = SplitKeyVal(line);

                    switch (pair.Key)
                    {
                        case "HitCirclePrefix":
                            skin.HitCircleFont = pair.Value;
                            break;
                    }

                    break;
                }
            }

            base.ParseLine(skin, section, line);
        }
    }
}
