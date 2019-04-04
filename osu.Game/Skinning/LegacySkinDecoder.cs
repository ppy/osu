// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            var pair = SplitKeyVal(line);
            switch (section)
            {
                case Section.General:
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

                case Section.Fonts:
                    switch (pair.Key)
                    {
                        case "HitCirclePrefix":
                            skin.HitCircleFont = pair.Value;
                            break;
                        case "HitCircleOverlap":
                            skin.HitCircleOverlap = int.Parse(pair.Value);
                            break;
                    }

                    break;
            }

            base.ParseLine(skin, section, line);
        }
    }
}
