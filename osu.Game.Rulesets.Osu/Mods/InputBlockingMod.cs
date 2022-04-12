// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{

    public abstract class InputBlockingMod : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer
    {

        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.Conversion;

        protected IFrameStableClock gameplayClock;
        protected IBindable<bool> isBreakTime;
        protected double firstObjectValidJudgementTime;
        protected const double flash_duration = 1000;

        protected OsuAction? lastActionPressed;

        protected DrawableRuleset<OsuHitObject> ruleset;

        public virtual bool checkCorrectAction(OsuAction action){
            if (isBreakTime.Value)
                return true;

            if (gameplayClock.CurrentTime < firstObjectValidJudgementTime)
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
            return false;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));

            OsuHitObject firstHitObject;
            try
            {
                firstHitObject = ruleset.Beatmap.HitObjects[0];
            }
            catch(IndexOutOfRangeException)
            {
                firstHitObject = null;
            }

            firstObjectValidJudgementTime = (firstHitObject?.StartTime ?? 0) - (firstHitObject?.HitWindows.WindowFor(HitResult.Meh) ?? 0);

            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void ApplyToPlayer(Player player)
        {
            isBreakTime = player.IsBreakTime.GetBoundCopy();
            isBreakTime.ValueChanged += e =>
            {
                if (e.NewValue)
                    lastActionPressed = null;
            };
        }


        private class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly InputBlockingMod mod;

            public InputInterceptor(InputBlockingMod mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
                => !mod.checkCorrectAction(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }
    }
}
