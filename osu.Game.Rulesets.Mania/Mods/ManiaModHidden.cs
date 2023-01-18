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

        public override BindableNumber<float> Coverage { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.1f,
            MinValue = 0.2f,
            MaxValue = 0.8f,
            Default = 0.5f,
        };

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ManiaModFadeIn)).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AgainstScroll;
    }
}
