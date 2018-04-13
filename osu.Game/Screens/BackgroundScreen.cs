// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using osu.Framework.Screens;
using osu.Framework.Graphics;
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

        private const float transition_length = 500;
        private const float x_movement_amount = 50;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            //we don't want to handle escape key.
            return false;
        }

        public override bool Push(Screen screen)
        {
            // When trying to push a non-loaded screen, load it asynchronously and re-invoke Push
            // once it's done.
            if (screen.LoadState == LoadState.NotLoaded)
            {
                LoadComponentAsync(screen, d => Push((BackgroundScreen)d));
                return true;
            }

            // Make sure the in-progress loading is complete before pushing the screen.
            while (screen.LoadState < LoadState.Ready)
                Thread.Sleep(1);

            base.Push(screen);

            return true;
        }

        protected override void Update()
        {
            base.Update();
            Content.Scale = new Vector2(1 + x_movement_amount / DrawSize.X * 2);
        }

        protected override void OnEntering(Screen last)
        {
            Content.FadeOut();
            Content.MoveToX(x_movement_amount);

            Content.FadeIn(transition_length, Easing.InOutQuart);
            Content.MoveToX(0, transition_length, Easing.InOutQuart);

            base.OnEntering(last);
        }

        protected override void OnSuspending(Screen next)
        {
            Content.MoveToX(-x_movement_amount, transition_length, Easing.InOutQuart);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            Content.FadeOut(transition_length, Easing.OutExpo);
            Content.MoveToX(x_movement_amount, transition_length, Easing.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            Content.MoveToX(0, transition_length, Easing.OutExpo);
            base.OnResuming(last);
        }
    }
}
