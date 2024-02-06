// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : ManiaModHidden
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override LocalisableString Description => @"Keys appear out of nowhere!";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ManiaModHidden)).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AlongScroll;
    }
}
