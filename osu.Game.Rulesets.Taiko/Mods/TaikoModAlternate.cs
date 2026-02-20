// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModAlternate : InputBlockingMod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't hit the same side twice in a row!";
        public override IconUsage? Icon => OsuIcon.ModAlternate;

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModSingleTap) }).ToArray();
        public override ModType Type => ModType.Conversion;
        public override bool Ranked => true;

        [SettingSource("Playstyle", "Change the playstyle used to determine alternating.", 1)]
        public Bindable<Playstyle> UserPlaystyle { get; } = new Bindable<Playstyle>();

        protected override bool CheckValidNewAction(TaikoAction action)
        {
            if (NonGameplayPeriods.IsInAny(GameplayClock.CurrentTime))
                return true;

            return UserPlaystyle.Value == Playstyle.KDDK ? checkCorrectActionKDDK(action) : checkCorrectActionDDKK(action);
        }

        private bool checkCorrectActionKDDK(TaikoAction action)
        {
            var currentHitObject = Playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.AllJudged != true)?.HitObject;

            // If next hit object is strong, a swell, or a drumroll, allow usage of all actions.
            // Since the player may lose place of which side they used last, we let them use either for the next note.
            if (currentHitObject is Swell || currentHitObject is DrumRoll || (currentHitObject is TaikoStrongableHitObject hitObject && hitObject.IsStrong))
            {
                LastAcceptedAction = null;
                return true;
            }

            switch (action)
            {
                case TaikoAction.LeftCentre or TaikoAction.LeftRim
                    when LastAcceptedAction == null || (LastAcceptedAction != TaikoAction.LeftCentre && LastAcceptedAction != TaikoAction.LeftRim):
                case TaikoAction.RightCentre or TaikoAction.RightRim
                    when LastAcceptedAction == null || (LastAcceptedAction != TaikoAction.RightCentre && LastAcceptedAction != TaikoAction.RightRim):
                    LastAcceptedAction = action;
                    return true;

                default:
                    return false;
            }
        }

        private bool checkCorrectActionDDKK(TaikoAction action)
        {
            var currentHitObject = Playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.AllJudged != true)?.HitObject;

            // Let players use any key on and after swells or drumrolls.
            if (currentHitObject is Swell or DrumRoll)
            {
                LastAcceptedCentreAction = null;
                LastAcceptedRimAction = null;

                return true;
            }

            // If the current hit object is strong, allow usage of all actions. Strong drum rolls are ignored in this check.
            // Since the player may lose place of which side they used last, we let them use either for the next note.
            if (currentHitObject is TaikoStrongableHitObject hitObject && hitObject.IsStrong)
            {
                // We reset the side that was hit because the other side should not have lost its place.
                if (action is TaikoAction.LeftCentre or TaikoAction.RightCentre)
                    LastAcceptedCentreAction = null;
                else if (action is TaikoAction.LeftRim or TaikoAction.RightRim)
                    LastAcceptedRimAction = null;

                return true;
            }

            switch (action)
            {
                case TaikoAction.LeftCentre when LastAcceptedCentreAction == null || LastAcceptedCentreAction != TaikoAction.LeftCentre:
                case TaikoAction.RightCentre when LastAcceptedCentreAction == null || LastAcceptedCentreAction != TaikoAction.RightCentre:
                    LastAcceptedCentreAction = action;
                    return true;

                case TaikoAction.LeftRim when LastAcceptedRimAction == null || LastAcceptedRimAction != TaikoAction.LeftRim:
                case TaikoAction.RightRim when LastAcceptedRimAction == null || LastAcceptedRimAction != TaikoAction.RightRim:
                    LastAcceptedRimAction = action;
                    return true;

                default:
                    return false;
            }
        }

        public enum Playstyle
        {
            KDDK,
            DDKK
        }
    }
}
