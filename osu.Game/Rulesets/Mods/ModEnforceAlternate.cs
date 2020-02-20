// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEnforceAlternate : Mod
    {
        public override string Name => "Enforce Alternate";
        public override string Acronym => "EA";
        public override IconUsage? Icon => FontAwesome.Solid.HandPeace;
        public override ModType Type => ModType.Fun;
        public override string Description => "You must alternate.";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };

        [SettingSource("Fail on repeat tap")]
        public Bindable<bool> CauseFail { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };
    }
}
