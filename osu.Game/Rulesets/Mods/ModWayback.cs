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
        public override double ScoreMultiplier
        {
            get
            {
                double multiplier = 1;
                float followSpeed = FollowSpeed.Value;
                double pos = 0;
                while (pos < 90)
                {
                    multiplier += 0.01;
                    pos += (100 - pos) / 100 * followSpeed;
                }
                return multiplier;
            }
        }
        public override Type[] IncompatibleMods => new Type[] { typeof(ModNoScope) };

        [SettingSource("Follow speed", "The speed at which the cursor follows your mouse.")]
        public BindableNumber<float> FollowSpeed { get; } = new BindableFloat(5)
        {
            MinValue = 2.5f,
            MaxValue = 15f,
            Precision = 0.01f,
        };
    }
}
