// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{

    public abstract class ModInputBlocker<TObject, TAction> : Mod, IApplicableToDrawableRuleset<TObject>, IApplicableToPlayer
        where TObject : HitObject
        where TAction : struct
    {
        public abstract bool checkCorrectAction(TAction action);

        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.Conversion;

        protected IFrameStableClock gameplayClock;
        protected IBindable<bool> isBreakTime;
        protected double firstObjectValidJudgementTime;
        protected const double flash_duration = 1000;

        protected TAction? lastActionPressed;

        protected DrawableRuleset<TObject> ruleset;

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));

            TObject firstHitObject;
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


        private class InputInterceptor : Component, IKeyBindingHandler<TAction>
        {
            private readonly ModInputBlocker<TObject, TAction> mod;

            public InputInterceptor(ModInputBlocker<TObject, TAction> mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<TAction> e)
                => !mod.checkCorrectAction(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<TAction> e)
            {
            }
        }
    }
}
