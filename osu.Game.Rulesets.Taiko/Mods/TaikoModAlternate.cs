// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
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
    public partial class TaikoModAlternate : Mod, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't hit the same side twice in a row!";
        public override IconUsage? Icon => OsuIcon.ModAlternate;

        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(TaikoModCinema), typeof(TaikoModSingleTap) };
        public override ModType Type => ModType.Conversion;
        public override bool Ranked => true;

        private DrawableTaikoRuleset ruleset = null!;

        private TaikoPlayfield playfield { get; set; } = null!;

        private TaikoAction? lastAcceptedAction { get; set; }
        private TaikoAction? lastAcceptedCenterAction { get; set; }
        private TaikoAction? lastAcceptedRimAction { get; set; }

        /// <summary>
        /// A tracker for periods where single tap should not be enforced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        private PeriodTracker nonGameplayPeriods = null!;

        private IFrameStableClock gameplayClock = null!;

        [SettingSource("Playstyle", "Change the playstyle used to determine alternating.", 1)]
        public Bindable<Playstyle> UserPlaystyle { get; } = new Bindable<Playstyle>(Playstyle.KDDK);

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

        public void Update(Playfield playfield)
        {
            if (!nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime)) return;

            lastAcceptedAction = null;
            lastAcceptedCenterAction = null;
            lastAcceptedRimAction = null;
        }

        private bool checkCorrectAction(TaikoAction action) => UserPlaystyle.Value == Playstyle.KDDK ? checkCorrectActionKDDK(action) : checkCorrectActionDDKK(action);

        private bool checkCorrectActionKDDK(TaikoAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            var currentHitObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject;

            // If next hit object is strong or a swell, allow usage of all actions. Strong drum rolls are ignored in this check.
            // Since the player may lose place of which side they used last, we let them use either for the next note.
            if (currentHitObject is Swell || (currentHitObject is TaikoStrongableHitObject hitObject && hitObject.IsStrong && hitObject is not DrumRoll))
            {
                lastAcceptedAction = null;
                return true;
            }

            if ((action == TaikoAction.LeftCentre || action == TaikoAction.LeftRim)
                && (lastAcceptedAction == null || (lastAcceptedAction != TaikoAction.LeftCentre && lastAcceptedAction != TaikoAction.LeftRim)))
            {
                lastAcceptedAction = action;
                return true;
            }

            if ((action == TaikoAction.RightCentre || action == TaikoAction.RightRim)
                && (lastAcceptedAction == null || (lastAcceptedAction != TaikoAction.RightCentre && lastAcceptedAction != TaikoAction.RightRim)))
            {
                lastAcceptedAction = action;
                return true;
            }

            return false;
        }

        private bool checkCorrectActionDDKK(TaikoAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            var currentHitObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(h => h.Result?.HasResult != true)?.HitObject;

            // Let players use any key on and after swells.
            if (currentHitObject is Swell)
            {
                lastAcceptedCenterAction = null;
                lastAcceptedRimAction = null;

                return true;
            }

            // If the current hit object is strong, allow usage of all actions. Strong drum rolls are ignored in this check.
            // Since the player may lose place of which side they used last, we let them use either for the next note.
            if (currentHitObject is TaikoStrongableHitObject hitObject && hitObject.IsStrong && hitObject is not DrumRoll)
            {
                // We reset the side that was hit because the other side should not have lost its place.
                if (action is TaikoAction.LeftCentre or TaikoAction.RightCentre)
                    lastAcceptedCenterAction = null;
                else if (action is TaikoAction.LeftRim or TaikoAction.RightRim)
                    lastAcceptedRimAction = null;

                return true;
            }

            if (action == TaikoAction.LeftCentre && (lastAcceptedCenterAction == null || lastAcceptedCenterAction != TaikoAction.LeftCentre))
            {
                lastAcceptedCenterAction = action;
                return true;
            }

            if (action == TaikoAction.RightCentre && (lastAcceptedCenterAction == null || lastAcceptedCenterAction != TaikoAction.RightCentre))
            {
                lastAcceptedCenterAction = action;
                return true;
            }

            if (action == TaikoAction.LeftRim && (lastAcceptedRimAction == null || lastAcceptedRimAction != TaikoAction.LeftRim))
            {
                lastAcceptedRimAction = action;
                return true;
            }

            if (action == TaikoAction.RightRim && (lastAcceptedRimAction == null || lastAcceptedRimAction != TaikoAction.RightRim))
            {
                lastAcceptedRimAction = action;
                return true;
            }

            return false;
        }

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

        public enum Playstyle
        {
            KDDK,
            DDKK
        }
    }
}
