// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDaycore : ModHalfTime
    {
        public override string Name => "Daycore";
        public override string Acronym => "DC";
        public override IconUsage Icon => FontAwesome.Solid.Question;
        public override string Description => "Whoaaaaa...";

        public override void ApplyToTrack(Track track)
        {
            track.Frequency.Value *= RateAdjust;
        }
    }
}
