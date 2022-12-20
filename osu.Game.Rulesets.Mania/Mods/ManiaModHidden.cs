// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHidden : ManiaModPlayfieldCover
    {
        public override LocalisableString Description => @"Keys fade out before you hit them!";
        public override double ScoreMultiplier => 1;

        [SettingSource("Coverage", "The proportion of playfield height that notes will be hidden for.")]
        public BindableNumber<float> CoverageAmount { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.01f,
            MinValue = 0.2f,
            MaxValue = 0.8f,
            Default = 0.5f,
        };

        protected override float Coverage => CoverageAmount.Value;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ManiaModFadeIn)).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AgainstScroll;
    }
}
