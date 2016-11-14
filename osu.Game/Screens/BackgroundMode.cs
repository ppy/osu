//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Screens
{
    public abstract class BackgroundMode : GameMode, IEquatable<BackgroundMode>
    {
        public virtual bool Equals(BackgroundMode other)
        {
            return other?.GetType() == GetType();
        }

        const float transition_length = 500;
        const float x_movement_amount = 50;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            //we don't want to handle escape key.
            return false;
        }

        BaseGame game;

        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            this.game = game;
        }

        public override bool Push(GameMode mode)
        {
            // When trying to push a non-loaded GameMode, load it asynchronously and re-invoke Push
            // once it's done.
            if (mode.LoadState == LoadState.NotLoaded)
            {
                mode.Preload(game, d => Push((BackgroundMode)d));
                return true;
            }

            // Make sure the in-progress loading is complete before pushing the GameMode.
            while (mode.LoadState < LoadState.Loaded)
                Thread.Sleep(1);

            base.Push(mode);

            return true;
        }

        protected override void Update()
        {
            base.Update();
            Content.Scale = new Vector2(1 + (x_movement_amount / DrawSize.X) * 2);
        }

        protected override void OnEntering(GameMode last)
        {
            Content.FadeOut();
            Content.MoveToX(x_movement_amount);

            Content.FadeIn(transition_length, EasingTypes.InOutQuart);
            Content.MoveToX(0, transition_length, EasingTypes.InOutQuart);

            base.OnEntering(last);
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.MoveToX(-x_movement_amount, transition_length, EasingTypes.InOutQuart);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
        {
            Content.FadeOut(transition_length, EasingTypes.OutExpo);
            Content.MoveToX(x_movement_amount, transition_length, EasingTypes.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnResuming(GameMode last)
        {
            Content.MoveToX(0, transition_length, EasingTypes.OutExpo);
            base.OnResuming(last);
        }
    }
}
