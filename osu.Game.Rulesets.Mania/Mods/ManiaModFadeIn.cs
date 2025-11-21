// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : ManiaModHidden
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModFadeIn;
        public override LocalisableString Description => @"Keys appear out of nowhere!";
        public override double ScoreMultiplier => 1;
        public override bool ValidForFreestyleAsRequiredMod => false;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(ManiaModHidden),
            typeof(ManiaModCover)
        }).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AlongScroll;
    }
}
