// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.UI;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHidden : ManiaModPlayfieldCover
    {
        public override LocalisableString Description => @"Keys fade out before you hit them!";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ManiaModFadeIn)).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AgainstScroll;

        public override BindableNumber<float> MinCoverage { get; } = new BindableFloat(0.3f)
        {
            Precision = 0.1f,
            MinValue = 0.2f,
            MaxValue = 0.8f,
            Default = 0.3f,
        };

        public override BindableNumber<float> MaxCoverage { get; } = new BindableFloat(0.6f)
        {
            Precision = 0.1f,
            MinValue = 0.2f,
            MaxValue = 0.8f,
            Default = 0.6f,
        };
    }
}
