// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAllCircles : Mod
    {
        public override string Name => "Circles Only";

        public override string Acronym => "CO";

        public override ModType Type => ModType.Conversion;

        public override LocalisableString Description => "Sliders? Never heard of them.";

        public override double ScoreMultiplier => ConvertEnds.Value ? 0.75 : 0.5;

        [SettingSource("Convert Ends", "Should slider repeats/ends become circles")]
        public virtual BindableBool ConvertEnds { get; } = new BindableBool();
    }
}
