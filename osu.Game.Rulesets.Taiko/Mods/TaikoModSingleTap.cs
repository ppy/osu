// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModSingleTap : Mod, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override LocalisableString Description => @"One key for dons, one key for kats.";

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(TaikoModCinema) };
        public override ModType Type => ModType.Conversion;

        private DrawableTaikoRuleset ruleset = null!;

        private TaikoPlayfield playfield { get; set; } = null!;

        private TaikoAction? lastAcceptedCentreAction { get; set; }
        private TaikoAction? lastAcceptedRimAction { get; set; }

        /// <summary>
        /// A tracker for periods where single tap should not be enforced (i.e. non-gameplay periods).
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

                static double getValidJudgementTime(HitObject hitObject) => hitObject.StartTime - hitObject.HitWindows.WindowFor(HitResult.Meh);
            }

            nonGameplayPeriods = new PeriodTracker(periods);

            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void Update(Playfield playfield)
        {
            if (!nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime)) return;

            lastAcceptedCentreAction = null;
            lastAcceptedRimAction = null;
        }

        private bool checkCorrectAction(TaikoAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            // If next hit object is strong, allow usage of all actions. Strong drumrolls are ignored in this check.
            if (playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject is TaikoStrongableHitObject hitObject
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

        private partial class InputInterceptor : Component, IKeyBindingHandler<TaikoAction>
        {
            private readonly TaikoModSingleTap mod;

            public InputInterceptor(TaikoModSingleTap mod)
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
