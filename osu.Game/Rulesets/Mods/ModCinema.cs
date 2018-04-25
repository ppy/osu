// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public class ModCinema : ModAutoplay
    {
        public override string Name => "Cinema";
        public override string ShortenedName => "CN";
        public override bool HasImplementation => false;
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_cinema;
        public override string Description => "Watch the video without visual distractions.";
    }
}
