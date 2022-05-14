// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override string Description => @"Don't use the same key multiple times in a row!";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax) };
        public override ModType Type => ModType.Conversion;
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;

        [SettingSource("Alternate after", "The maximum times the same key can be pressed")]
        public BindableNumber<int> MaxTimes { get; } = new BindableInt
        {
            MinValue = 1,
            MaxValue = 10,
            Default = 1,
            Value = 1,
            Precision = 1
        };

        private const double flash_duration = 1000;

        /// <summary>
        /// A tracker for periods where alternate should not be forced (i.e. non-gameplay periods).
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="Player.IsBreakTime"/> in that the periods here end strictly at the first object after the break, rather than the break's end time.
        /// </remarks>
        private PeriodTracker nonGameplayPeriods;

        private OsuAction? lastActionPressed;
        private int numActions;
        private DrawableRuleset<OsuHitObject> ruleset;

        private IFrameStableClock gameplayClock;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));

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

        private bool checkCorrectAction(OsuAction action)
        {
            if (nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
            {
                lastActionPressed = null;
                return true;
            }

            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    break;

                // Any action which is not left or right button should be ignored.
                default:
                    return true;
            }

            if (lastActionPressed != action)
            {
                // User alternated correctly.
                lastActionPressed = action;
                numActions = 0;
                return true;
            }

            if (lastActionPressed == action)
            {
                // If same action, increment counter.
                numActions++;

                if (numActions < MaxTimes.Value)
                {
                    // Number of repeated actions is less than the maximum, so ignore.
                    return true;
                }
            }

            ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
            return false;
        }

        private class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly OsuModAlternate mod;

            public InputInterceptor(OsuModAlternate mod)
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
