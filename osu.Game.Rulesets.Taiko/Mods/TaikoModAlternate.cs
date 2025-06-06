// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModAlternate : InputBlockingMod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't use the same side twice in a row!";

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModSingleTap) }).ToArray();

        [SettingSource("Alternate Fingers", "For ddkk or kkdd players who alternate fingers instead of hands.")]
        public Bindable<bool> AlternateFingers { get; } = new BindableBool();

        private Side? lastAcceptedSide;
        private TaikoAction? lastAcceptedAction;

        private readonly Dictionary<Side, TaikoAction[]> sideActions = new Dictionary<Side, TaikoAction[]>()
        {
            [Side.Left] = [TaikoAction.LeftCentre, TaikoAction.LeftRim],
            [Side.Right] = [TaikoAction.RightCentre, TaikoAction.RightRim],
        };

        public override void Reset()
        {
            lastAcceptedSide = null;
            lastAcceptedAction = null;
        }

        protected override bool CheckCorrectAction(TaikoAction action)
        {
            // Some objects are traditionally ignore for alternating and thus allows you to reset your alternation pattern.
            bool altReset = false;
            TaikoHitObject? nextHitObject = GetNextHitObject()?.HitObject;
            TaikoHitObject? lastHitObject = getLastHitObject()?.HitObject;

            // The most significant example being strong hits, which requires both sides to hit.
            altReset |= nextHitObject is TaikoStrongableHitObject nextStrongHitObject && nextStrongHitObject.IsStrong;
            altReset |= lastHitObject is TaikoStrongableHitObject lastStrongHitObject && lastStrongHitObject.IsStrong;

            // Swells are often played by tapping dk on one hand, and then dk on the other, in a fast "rolling" fashion.
            // Drumrolls are rarer but the same idea applies.
            altReset |= nextHitObject is Swell or DrumRoll;
            altReset |= lastHitObject is Swell or DrumRoll;

            if (altReset)
            {
                lastAcceptedSide = null;
                lastAcceptedAction = null;
                return true;
            }

            // If there's no previous side, accept everything.
            if (lastAcceptedSide == null)
            {
                lastAcceptedSide = getSideForAction(action);
                lastAcceptedAction = action;
                return true;
            }

            if (AlternateFingers.Value)
            {
                if (action != lastAcceptedAction)
                {
                    lastAcceptedAction = action;
                    return true;
                }
            }
            else
            {
                Side targetSide = getOppositeSide(lastAcceptedSide.Value);
                TaikoAction[] acceptableActions = sideActions[targetSide];

                if (acceptableActions.Contains(action))
                {
                    lastAcceptedSide = targetSide;
                    return true;
                }
            }

            return false;
        }

        private DrawableTaikoHitObject? getLastHitObject()
        {
            DrawableHitObject? hitObject = Playfield.HitObjectContainer.AliveObjects.LastOrDefault(h => h.Result?.HasResult == true);
            return (DrawableTaikoHitObject?)hitObject;
        }

        private Side getSideForAction(TaikoAction action) => sideActions[Side.Left].Contains(action) ? Side.Left : Side.Right;

        private enum Side
        {
            Left, Right
        }

        private Side getOppositeSide(Side side) => side == Side.Left ? Side.Right : Side.Left;
    }
}
