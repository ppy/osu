// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public abstract partial class InputBlockingMod : Mod, IApplicableToDrawableRuleset<TaikoHitObject>, IUpdatableByPlayfield
    {
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(TaikoModCinema) };
        public override ModType Type => ModType.Conversion;

        private DrawableTaikoRuleset ruleset = null!;

        protected TaikoPlayfield Playfield { get; set; } = null!;

        protected TaikoAction? LastAcceptedAction { get; set; }
        protected TaikoAction? LastAcceptedCentreAction { get; set; }
        protected TaikoAction? LastAcceptedRimAction { get; set; }

        /// <summary>
        /// A tracker for periods where alternate should not be forced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        protected PeriodTracker NonGameplayPeriods = null!;

        protected IFrameStableClock GameplayClock = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            ruleset = (DrawableTaikoRuleset)drawableRuleset;
            ruleset.KeyBindingInputManager.Add(new InputInterceptor(this));
            Playfield = (TaikoPlayfield)ruleset.Playfield;

            var periods = new List<Period>();

            if (drawableRuleset.Objects.Any())
            {
                periods.Add(new Period(int.MinValue, getValidJudgementTime(ruleset.Objects.First()) - 1));

                foreach (BreakPeriod b in drawableRuleset.Beatmap.Breaks)
                    periods.Add(new Period(b.StartTime, getValidJudgementTime(ruleset.Objects.First(h => h.StartTime >= b.EndTime)) - 1));

                static double getValidJudgementTime(HitObject hitObject) => hitObject.StartTime - hitObject.HitWindows.WindowFor(HitResult.Ok);
            }

            NonGameplayPeriods = new PeriodTracker(periods);

            GameplayClock = drawableRuleset.FrameStableClock;
        }

        public void Update(Playfield playfield)
        {
            if (LastAcceptedAction != null && NonGameplayPeriods.IsInAny(GameplayClock.CurrentTime))
            {
                LastAcceptedAction = null;
                LastAcceptedCentreAction = null;
                LastAcceptedRimAction = null;
            }

            if (LastAcceptedAction != null && GameplayClock.IsRewinding)
            {
                LastAcceptedAction = null;
                LastAcceptedCentreAction = null;
                LastAcceptedRimAction = null;
            }
        }

        protected abstract bool CheckValidNewAction(TaikoAction action);

        private bool checkCorrectAction(TaikoAction action)
        {
            if (NonGameplayPeriods.IsInAny(GameplayClock.CurrentTime))
                return true;

            return CheckValidNewAction(action);
        }

        private partial class InputInterceptor : Component, IKeyBindingHandler<TaikoAction>
        {
            private readonly InputBlockingMod mod;

            public InputInterceptor(InputBlockingMod mod)
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
