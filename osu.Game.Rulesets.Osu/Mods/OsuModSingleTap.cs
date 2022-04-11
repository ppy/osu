// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"ST";
        public override string Description => @"Only use one key!";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAlternate), typeof(OsuModRelax) };
        public override ModType Type => ModType.Conversion;
        private double firstObjectValidJudgementTime;
        private IBindable<bool> isBreakTime;
        private const double flash_duration = 1000;
        private OsuAction? lastActionPressed;
        private DrawableRuleset<OsuHitObject> ruleset;

        private IFrameStableClock gameplayClock;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));

            var firstHitObject = ruleset.Objects.FirstOrDefault();
            firstObjectValidJudgementTime = (firstHitObject?.StartTime ?? 0) - (firstHitObject?.HitWindows.WindowFor(HitResult.Meh) ?? 0);

            gameplayClock = drawableRuleset.FrameStableClock;
        }

        public void ApplyToPlayer(Player player)
        {
            isBreakTime = player.IsBreakTime.GetBoundCopy();
        }

        private bool checkCorrectAction(OsuAction action)
        {
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

            if (lastActionPressed == null)
            {
                lastActionPressed = action;
                return true;
            }
            else if (lastActionPressed == action)
            {
                return true;
            }

            ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
            return false;
        }

        private class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly OsuModSingleTap mod;

            public InputInterceptor(OsuModSingleTap mod)
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
