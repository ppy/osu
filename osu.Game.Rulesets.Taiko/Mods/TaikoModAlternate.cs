// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
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

        [SettingSource("Playstyle", "Preferred alternate playstyle")]
        public Bindable<Playstyle> Style { get; } = new Bindable<Playstyle>();

        private Side? lastAcceptedSide;
        private TaikoAction? lastAcceptedAction;

        private readonly Dictionary<Side, TaikoAction[]> sideActions = new Dictionary<Side, TaikoAction[]>
        {
            [Side.Left] = [TaikoAction.LeftCentre, TaikoAction.LeftRim],
            [Side.Right] = [TaikoAction.RightCentre, TaikoAction.RightRim],
        };

        public override void Reset()
        {
            lastAcceptedSide = null;
            lastAcceptedAction = null;
        }

        private bool shouldAltReset()
        {
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

            return altReset;
        }

        protected override bool CheckCorrectAction(TaikoAction action)
        {
            // Some objects are traditionally ignored for alternating and thus allows you to reset your alternation pattern.
            if (shouldAltReset())
            {
                Reset();
                return true;
            }

            // If there's no previous state, accept everything.
            if (lastAcceptedSide == null || lastAcceptedAction == null)
            {
                lastAcceptedSide = getSideForAction(action);
                lastAcceptedAction = action;
                return true;
            }

            switch (Style.Value)
            {
                case Playstyle.AlternateFingers:
                    if (action != lastAcceptedAction)
                    {
                        lastAcceptedSide = getSideForAction(action);
                        lastAcceptedAction = action;
                        return true;
                    }

                    break;

                case Playstyle.AlternateHands:
                    Side targetSide = getOppositeSide(lastAcceptedSide.Value);
                    TaikoAction[] acceptableActions = sideActions[targetSide];

                    if (acceptableActions.Contains(action))
                    {
                        lastAcceptedSide = targetSide;
                        lastAcceptedAction = action;
                        return true;
                    }

                    break;
            }

            return false;
        }

        private DrawableTaikoHitObject? getLastHitObject()
        {
            DrawableHitObject? hitObject = Playfield.HitObjectContainer.AliveObjects.LastOrDefault(h => h.Result?.HasResult == true);
            return (DrawableTaikoHitObject?)hitObject;
        }

        private Side getSideForAction(TaikoAction action) => sideActions[Side.Left].Contains(action) ? Side.Left : Side.Right;
        private Side getOppositeSide(Side side) => side == Side.Left ? Side.Right : Side.Left;

        public enum Playstyle
        {
            /// <summary>
            /// Each hand has a rim and a centre button, so alternating is done by changing hands.
            /// Also known in community vernacular as "kddk".
            /// </summary>
            [Description(@"Alternate hands (""kddk"")")]
            AlternateHands,

            /// <summary>
            /// One hand has both rim buttons and the other both centre buttons.
            /// Alternating is done using fingers only.
            /// Also known in community vernacular as "kkdd".
            /// </summary>
            [Description(@"Alternate fingers (""kkdd"")")]
            AlternateFingers
        }

        private enum Side
        {
            Left, Right
        }
    }
}
