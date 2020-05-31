// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : ModRateAdjust
    {
        public override string Name => "Half Time";
        public override string Acronym => "HT";
        public override IconUsage? Icon => OsuIcon.ModHalftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Less zoom...";
        public override bool Ranked => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModDoubleTime)).ToArray();

        [SettingSource("Speed decrease", "The actual decrease to apply")]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Default = 0.75,
            Value = 0.75,
            Precision = 0.01,
        };
    }
}
