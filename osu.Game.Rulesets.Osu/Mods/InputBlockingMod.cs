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
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public abstract partial class InputBlockingMod : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IUpdatableByPlayfield
    {
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(OsuModCinema) };
        public override ModType Type => ModType.Conversion;

        private const double flash_duration = 1000;

        private DrawableOsuRuleset ruleset = null!;

        protected OsuAction? LastAcceptedAction { get; private set; }

        /// <summary>
        /// A tracker for periods where alternate should not be forced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        private PeriodTracker nonGameplayPeriods = null!;

        private IFrameStableClock gameplayClock = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = (DrawableOsuRuleset)drawableRuleset;
            ruleset.KeyBindingInputManager.Add(new InputInterceptor(this));

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
            if (LastAcceptedAction != null && nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                LastAcceptedAction = null;
        }

        protected abstract bool CheckValidNewAction(OsuAction action);

        private bool checkCorrectAction(OsuAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                return true;

            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    break;

                // Any action which is not left or right button should be ignored.
                default:
                    return true;
            }

            if (CheckValidNewAction(action))
            {
                LastAcceptedAction = action;
                return true;
            }

            ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
            return false;
        }

        private partial class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly InputBlockingMod mod;

            public InputInterceptor(InputBlockingMod mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
                // if the pressed action is incorrect, block it from reaching gameplay.
                => !mod.checkCorrectAction(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }
    }
}
