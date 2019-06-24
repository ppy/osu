// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public class ModCinema : ModAutoplay
    {
        public override string Name => "Cinema";
        public override string Acronym => "CN";
        public override bool HasImplementation => false;
        public override IconUsage Icon => OsuIcon.ModCinema;
        public override string Description => "Watch the video without visual distractions.";
    }
}
