// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class CircleDanceTransformerMod : OsuModCipher
    {
        public override string Name => "Circle Dance";
        public override string Acronym => "CD";
        public override LocalisableString Description => "Spins circles around original position";

        public override IconUsage? Icon => FontAwesome.Solid.CompressArrowsAlt;


        [SettingSource("Circle radius", "The radius the cursor goes around", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> CircleRadius { get; } = new BindableDouble(100)
        {
            MinValue = 1,
            MaxValue = 300,
            Precision = 5,
        };

        [SettingSource("Spin speed", "Cursor spin speed", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> Speed { get; } = new BindableDouble(2)
        {
            MinValue = 0.1,
            MaxValue = 10,
            Precision = 0.1,
        };

        private double arc;

        public override Vector2 Transform(Vector2 mousePosition)
        {
            arc += Speed.Value;
            float x = (float)(CircleRadius.Value * Math.Cos(arc));
            float y = (float)(CircleRadius.Value * Math.Sin(arc));
            return mousePosition + new Vector2(x, y);
        }
    }
}
