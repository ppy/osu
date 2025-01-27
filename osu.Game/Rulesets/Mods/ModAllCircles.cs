// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAllCircles : Mod
    {
        public override string Name => "All Circles";

        public override string Acronym => "CC";

        public override ModType Type => ModType.Conversion;

        public override LocalisableString Description => "Oops! All Circles! Sliders get changed into circles.";

        public override double ScoreMultiplier => 0.5;

        [SettingSource("Convert Ends", "Should slider repeats/ends be converted")]
        public virtual BindableBool ConvertEnds { get; } = new BindableBool();
    }
}
