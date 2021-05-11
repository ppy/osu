// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindDown : ModTimeRamp
    {
        public override string Name => "Wind Down";
        public override string Acronym => "WD";
        public override string Description => "Sloooow doooown...";
        public override IconUsage? Icon => FontAwesome.Solid.ChevronCircleDown;
        public override double ScoreMultiplier => 1.0;

        [SettingSource("Initial rate", "The starting speed of the track")]
        public override BindableNumber<double> InitialRate { get; } = new BindableDouble
        {
            MinValue = 0.51,
            MaxValue = 2,
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        [SettingSource("Final rate", "The speed increase to ramp towards")]
        public override BindableNumber<double> FinalRate { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 1.99,
            Default = 0.75,
            Value = 0.75,
            Precision = 0.01,
        };

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public override BindableBool AdjustPitch { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModWindUp)).ToArray();

        public ModWindDown()
        {
            InitialRate.BindValueChanged(val =>
            {
                if (val.NewValue <= FinalRate.Value)
                    FinalRate.Value = val.NewValue - FinalRate.Precision;
            });

            FinalRate.BindValueChanged(val =>
            {
                if (val.NewValue >= InitialRate.Value)
                    InitialRate.Value = val.NewValue + InitialRate.Precision;
            });
        }
    }
}
