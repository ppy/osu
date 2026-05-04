// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModPreciseTapping : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IUpdatableByPlayfield
    {
        public override string Name => @"Precise Tapping";
        public override string Acronym => @"PT";
        public override LocalisableString Description => @"Extra key presses count as misses.";
        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.Conversion;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax), typeof(OsuModCinema) };

        private DrawableOsuRuleset ruleset = null!;

        private PeriodTracker nonGameplayPeriods = null!;

        private IFrameStableClock gameplayClock = null!;

        private bool penaltyActive = true;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = (DrawableOsuRuleset)drawableRuleset;
            ruleset.Playfield.AttachInputInterceptor(new InputInterceptor(this));

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
            if (gameplayClock.IsRewinding || nonGameplayPeriods.IsInAny(gameplayClock.CurrentTime))
                penaltyActive = true;
        }

        private DrawableHitCircle? getNextTappable()
        {
            foreach (var alive in ruleset.Playfield.HitObjectContainer.AliveObjects)
            {
                DrawableHitCircle? circle = alive switch
                {
                    DrawableSlider slider => slider.HeadCircle,
                    DrawableHitCircle hitCircle => hitCircle,
                    _ => null
                };

                if (circle != null && circle.Result?.HasResult != true)
                    return circle;
            }

            return null;
        }

        private partial class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly OsuModPreciseTapping mod;

            public InputInterceptor(OsuModPreciseTapping mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                if (e.Action != OsuAction.LeftButton && e.Action != OsuAction.RightButton)
                    return false;

                if (mod.nonGameplayPeriods.IsInAny(mod.gameplayClock.CurrentTime))
                    return false;

                if (mod.gameplayClock.IsRewinding)
                    return false;

                var tappable = mod.getNextTappable();

                if (tappable != null)
                {
                    bool wasActive = mod.penaltyActive;

                    Scheduler.Add(() =>
                    {
                        if (tappable.Result?.IsHit == true)
                        {
                            mod.penaltyActive = true;
                            return;
                        }

                        if (tappable.Result?.HasResult == true)
                            return;

                        if (!wasActive)
                            return;

                        tappable.MissForcefully();
                        mod.penaltyActive = false;
                    });
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }
    }
}
