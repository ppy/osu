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
    public class OsuModAlternate : Mod, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToPlayer, IUpdatableByPlayfield
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override string Description => @"Don't use the same key twice in a row!";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
        public override ModType Type => ModType.DifficultyIncrease;
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;

        /// <summary>
        /// Whether incoming input must be checked by <see cref="InputInterceptor"/> before it is passed to gameplay.
        /// </summary>
        public bool CanIntercept => !isBreakTime.Value && introEnded;

        private bool introEnded;
        private double earliestStartTime;
        private IBindable<bool> isBreakTime;
        private const double flash_duration = 1000;
        private OsuAction? lastActionPressed;
        private DrawableRuleset<OsuHitObject> ruleset;

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            ruleset = drawableRuleset;
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));

            var firstHitObject = ruleset.Objects.FirstOrDefault();
            earliestStartTime = (firstHitObject?.StartTime ?? 0) - (firstHitObject?.HitWindows.WindowFor(HitResult.Meh) ?? 0);
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

        private bool onPressed(OsuAction key)
        {
            if (lastActionPressed == key)
            {
                ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
                return true;
            }

            lastActionPressed = key;

            return false;
        }

        public void Update(Playfield playfield)
        {
            if (!introEnded)
                introEnded = playfield.Clock.CurrentTime > earliestStartTime;
        }

        private class InputInterceptor : Component, IKeyBindingHandler<OsuAction>
        {
            private readonly OsuModAlternate mod;

            public InputInterceptor(OsuModAlternate mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
                => mod.CanIntercept && mod.onPressed(e.Action);

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }
    }
}
