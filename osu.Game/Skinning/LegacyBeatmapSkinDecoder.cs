// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Skinning
{
    public class LegacyBeatmapSkinDecoder : LegacySkinDecoder
    {
        protected override void ParseLine(DefaultSkinConfiguration skin, Section section, string line)
        {
            // Early return on "Version" key to disallow conflicting between .osu file beatmap difficulty version and skin configuration legacy version.
            if (SplitKeyVal(line).Key == "Version")
                return;

            base.ParseLine(skin, section, line);
        }
    }
}
