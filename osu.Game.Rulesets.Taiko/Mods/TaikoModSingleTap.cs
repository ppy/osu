// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Linq;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModSingleTap : InputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override LocalisableString Description => @"One key for dons, one key for kats.";

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModAlternate) }).ToArray();

        private TaikoAction? lastAcceptedCentreAction { get; set; }
        private TaikoAction? lastAcceptedRimAction { get; set; }

        public override void Reset()
        {
            lastAcceptedCentreAction = null;
            lastAcceptedRimAction = null;
        }

        protected override bool CheckCorrectAction(TaikoAction action)
        {
            // If next hit object is strong, allow usage of all actions. Strong drumrolls are ignored in this check.
            if (GetNextHitObject()?.HitObject is TaikoStrongableHitObject hitObject
                && hitObject.IsStrong
                && hitObject is not DrumRoll)
                return true;

            if ((action == TaikoAction.LeftCentre || action == TaikoAction.RightCentre)
                && (lastAcceptedCentreAction == null || lastAcceptedCentreAction == action))
            {
                lastAcceptedCentreAction = action;
                return true;
            }

            if ((action == TaikoAction.LeftRim || action == TaikoAction.RightRim)
                && (lastAcceptedRimAction == null || lastAcceptedRimAction == action))
            {
                lastAcceptedRimAction = action;
                return true;
            }

            return false;
        }
    }
}
