// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAlternate : Mod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override string Description => @"Never hit the same key twice!";
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
        public override ModType Type => ModType.DifficultyIncrease;
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;
    }

    public abstract class ModAlternate<THitObject, TAction> : ModAlternate, IApplicableToDrawableRuleset<THitObject>, IApplicableToPlayer
        where THitObject : HitObject
        where TAction : struct
    {
        public bool CanIntercept => !isBreakTime.Value;

        private IBindable<bool> isBreakTime;

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            drawableRuleset.KeyBindingInputManager.Add(new InputInterceptor(this));
        }

        public void ApplyToPlayer(Player player)
        {
            isBreakTime = player.IsBreakTime.GetBoundCopy();
            isBreakTime.ValueChanged += e =>
            {
                if (e.NewValue)
                    Reset();
            };
        }

        protected abstract void Reset();

        protected abstract bool OnPressed(TAction key);

        protected abstract void OnReleased(TAction key);

        private class InputInterceptor : Component, IKeyBindingHandler<TAction>
        {
            private readonly ModAlternate<THitObject, TAction> mod;

            public InputInterceptor(ModAlternate<THitObject, TAction> mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(KeyBindingPressEvent<TAction> e)
            {
                return mod.CanIntercept && mod.OnPressed(e.Action);
            }

            public void OnReleased(KeyBindingReleaseEvent<TAction> e)
            {
                if (mod.CanIntercept)
                    mod.OnReleased(e.Action);
            }
        }
    }
}
