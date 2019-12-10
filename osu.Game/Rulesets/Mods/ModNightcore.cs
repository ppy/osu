// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore : ModDoubleTime
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override IconUsage Icon => OsuIcon.ModNightcore;
        public override string Description => "Uguuuuuuuu...";

        public override void ApplyToTrack(Track track)
        {
            track.Frequency.Value *= RateAdjust;
        }
    }
}
