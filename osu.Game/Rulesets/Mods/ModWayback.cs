// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModWayback : Mod
    {
        public override string Name => "Wayback";
        public override string Acronym => "WB";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new Type[] { typeof(ModNoScope) };

        [SettingSource("Delay", "The delay (in seconds) of your cursor movement.")]
        public BindableNumber<float> Delay { get; } = new BindableFloat(0.3f)
        {
            MinValue = 0.3f,
            MaxValue = 5f,
            Precision = 0.1f,
        };
    }
}
