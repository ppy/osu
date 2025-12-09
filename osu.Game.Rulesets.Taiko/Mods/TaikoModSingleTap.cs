// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModSingleTap : InputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override IconUsage? Icon => OsuIcon.ModSingleTap;
        public override LocalisableString Description => @"One key for dons, one key for kats.";

        public override bool Ranked => true;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModAlternate) }).ToArray();
        public override ModType Type => ModType.Conversion;

        protected override bool CheckValidNewAction(TaikoAction action)
        {
            if (NonGameplayPeriods.IsInAny(GameplayClock.CurrentTime))
                return true;

            // If next hit object is strong, allow usage of all actions. Strong drumrolls are ignored in this check.
            if (Playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject is TaikoStrongableHitObject hitObject
                && hitObject.IsStrong
                && hitObject is not DrumRoll)
                return true;

            if ((action == TaikoAction.LeftCentre || action == TaikoAction.RightCentre)
                && (LastAcceptedCentreAction == null || LastAcceptedCentreAction == action))
            {
                LastAcceptedCentreAction = action;
                return true;
            }

            if ((action == TaikoAction.LeftRim || action == TaikoAction.RightRim)
                && (LastAcceptedRimAction == null || LastAcceptedRimAction == action))
            {
                LastAcceptedRimAction = action;
                return true;
            }

            return false;
        }
    }
}
