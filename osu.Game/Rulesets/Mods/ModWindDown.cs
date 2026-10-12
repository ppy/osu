// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindDown : ModTimeRamp
    {
        public override string Name => "Wind Down";
        public override string Acronym => "WD";
        public override LocalisableString Description => "Sloooow doooown...";
        public override IconUsage? Icon => OsuIcon.ModWindDown;

        public override BindableNumber<double> InitialRate { get; } = new BindableDouble(1)
        {
            MinValue = 0.51,
            MaxValue = 2,
            Precision = 0.01,
        };

        public override BindableNumber<double> FinalRate { get; } = new BindableDouble(0.75)
        {
            MinValue = 0.5,
            MaxValue = 1.99,
            Precision = 0.01,
        };

        public override BindableBool AdjustPitch { get; } = new BindableBool(true);

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
