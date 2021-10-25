﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens
{
    public abstract class BackgroundScreen : Screen, IEquatable<BackgroundScreen>
    {
        protected const float TRANSITION_LENGTH = 500;
        private const float x_movement_amount = 50;

        private readonly bool animateOnEnter;

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        protected BackgroundScreen(bool animateOnEnter = true)
        {
            this.animateOnEnter = animateOnEnter;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public virtual bool Equals(BackgroundScreen other)
        {
            return other?.GetType() == GetType();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // we don't want to handle escape key.
            return false;
        }

        /// <summary>
        /// Apply arbitrary changes to this background in a thread safe manner.
        /// </summary>
        /// <param name="action">The operation to perform.</param>
        public void ApplyToBackground(Action<BackgroundScreen> action) => Schedule(() => action.Invoke(this));

        protected override void Update()
        {
            base.Update();
            Scale = new Vector2(1 + x_movement_amount / DrawSize.X * 2);
        }

        public override void OnEntering(IScreen last)
        {
            if (animateOnEnter)
            {
                this.FadeOut();
                this.MoveToX(x_movement_amount);

                this.FadeIn(TRANSITION_LENGTH, Easing.InOutQuart);
                this.MoveToX(0, TRANSITION_LENGTH, Easing.InOutQuart);
            }

            base.OnEntering(last);
        }

        public override void OnSuspending(IScreen next)
        {
            this.MoveToX(-x_movement_amount, TRANSITION_LENGTH, Easing.InOutQuart);
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            if (IsLoaded)
            {
                this.FadeOut(TRANSITION_LENGTH, Easing.OutExpo);
                this.MoveToX(x_movement_amount, TRANSITION_LENGTH, Easing.OutExpo);
            }

            return base.OnExiting(next);
        }

        public override void OnResuming(IScreen last)
        {
            if (IsLoaded)
                this.MoveToX(0, TRANSITION_LENGTH, Easing.OutExpo);
            base.OnResuming(last);
        }
    }
}
