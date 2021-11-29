// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;


namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModColourApproach: Mod, IHidesApproachCircles
    {
        public override string Name => "Colour Approach";
        public override string Acronym => "CA";
        public override string Description => "Something about colours and such...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.VolumeMute;


        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles) };

        
    }
}