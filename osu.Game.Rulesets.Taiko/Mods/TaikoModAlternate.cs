// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModAlternate : Mod, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't use the same side twice in a row!";

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(TaikoModCinema), typeof(TaikoModSingleTap) };
        public override ModType Type => ModType.Conversion;

        [SettingSource("Alternate Fingers", "For ddkk or kkdd players who alternate fingers instead of hands.")]
        public Bindable<bool> AlternateFingers { get; } = new BindableBool();

        private DrawableTaikoRuleset ruleset = null!;

        private TaikoPlayfield playfield { get; set; } = null!;

        /// <summary>
        /// A tracker for periods where alternation should not be enforced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        private PeriodTracker nonGameplayPeriods = null!;

        private IFrameStableClock gameplayClock = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            ruleset = (DrawableTaikoRuleset)drawableRuleset;
            ruleset.KeyBindingInputManager.Add(new InputInterceptor(this));
            playfield = (TaikoPlayfield)ruleset.Playfield;

            var periods = new List<Period>();

            if (drawableRuleset.Objects.Any())
            {
                periods.Add(new Period(int.MinValue, getValidJudgementTime(ruleset.Objects.First()) - 1));

                foreach (BreakPeriod b in drawableRuleset.Beatmap.Breaks)
                    periods.Add(new Period(b.StartTime, getValidJudgementTime(ruleset.Objects.First(h => h.StartTime >= b.EndTime)) - 1));

                static double getValidJudgementTime(HitObject hitObject) => hitObject.StartTime - hitObject.HitWindows.WindowFor(HitResult.Ok);
            }

            nonGameplayPeriods = new PeriodTracker(periods);

            gameplayClock = drawableRuleset.FrameStableClock;
        }

        private Side? lastAcceptedSide;
        private TaikoAction? lastAcceptedAction;

        private readonly Dictionary<Side, TaikoAction[]> sideActions = new Dictionary<Side, TaikoAction[]>()
        {
            [Side.Left] = [TaikoAction.LeftCentre, TaikoAction.LeftRim],
            [Side.Right] = [TaikoAction.RightCentre, TaikoAction.RightRim],
        };

        public void Update(Playfield playfield)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
            {
                lastAcceptedSide = null;
                lastAcceptedAction = null;
            }
        }

        private bool checkCorrectAction(TaikoAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            // Some objects are traditionally ignore for alternating and thus allows you to reset your alternation pattern.
            bool altReset = false;
            TaikoHitObject? nextHitObject = getNextHitObject()?.HitObject;
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

        private DrawableTaikoHitObject? getNextHitObject()
        {
            DrawableHitObject? hitObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true);
            return (DrawableTaikoHitObject?)hitObject;
        }

        private DrawableTaikoHitObject? getLastHitObject()
        {
            DrawableHitObject? hitObject = playfield.HitObjectContainer.AliveObjects.LastOrDefault(h => h.Result?.HasResult == true);
            return (DrawableTaikoHitObject?)hitObject;
        }

        private Side getSideForAction(TaikoAction action) => sideActions[Side.Left].Contains(action) ? Side.Left : Side.Right;

        private enum Side
        {
            Left, Right
        }

        private Side getOppositeSide(Side side) => side == Side.Left ? Side.Right : Side.Left;

        private partial class InputInterceptor : Component, IKeyBindingHandler<TaikoAction>
        {
            private readonly TaikoModAlternate mod;

            public InputInterceptor(TaikoModAlternate mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
                // if the pressed action is incorrect, block it from reaching gameplay.
                => !mod.checkCorrectAction(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
            }
        }
    }
}
