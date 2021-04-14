// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBarrelRoll : Mod, IUpdatableByPlayfield
    {
        [SettingSource("Roll speed", "Speed at which things rotate")]
        public BindableNumber<double> SpinSpeed { get; } = new BindableDouble(1)
        {
            MinValue = 0.1,
            MaxValue = 20,
            Precision = 0.1,
        };

        public override string Name => "Barrel Roll";
        public override string Acronym => "BR";
        public override double ScoreMultiplier => 1;

        public void Update(Playfield playfield)
        {
            playfield.Rotation = (float)(playfield.Time.Current / 1000 * SpinSpeed.Value);
        }
    }
}
