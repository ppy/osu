﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Screens
{
    public abstract class BackgroundScreen : Screen, IEquatable<BackgroundScreen>
    {
        public virtual bool Equals(BackgroundScreen other)
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

        Framework.Game game;

        [BackgroundDependencyLoader]
        private void load(Framework.Game game)
        {
            this.game = game;
        }

        public override bool Push(Screen screen)
        {
            // When trying to push a non-loaded GameMode, load it asynchronously and re-invoke Push
            // once it's done.
            if (screen.LoadState == LoadState.NotLoaded)
            {
                screen.LoadAsync(game, d => Push((BackgroundScreen)d));
                return true;
            }

            // Make sure the in-progress loading is complete before pushing the GameMode.
            while (screen.LoadState < LoadState.Loaded)
                Thread.Sleep(1);

            base.Push(screen);

            return true;
        }

        protected override void Update()
        {
            base.Update();
            Content.Scale = new Vector2(1 + (x_movement_amount / DrawSize.X) * 2);
        }

        protected override void OnEntering(Screen last)
        {
            Content.FadeOut();
            Content.MoveToX(x_movement_amount);

            Content.FadeIn(transition_length, EasingTypes.InOutQuart);
            Content.MoveToX(0, transition_length, EasingTypes.InOutQuart);

            base.OnEntering(last);
        }

        protected override void OnSuspending(Screen next)
        {
            Content.MoveToX(-x_movement_amount, transition_length, EasingTypes.InOutQuart);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            Content.FadeOut(transition_length, EasingTypes.OutExpo);
            Content.MoveToX(x_movement_amount, transition_length, EasingTypes.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            Content.MoveToX(0, transition_length, EasingTypes.OutExpo);
            base.OnResuming(last);
        }
    }
}
