// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens
{
    public abstract partial class BackgroundScreen : Screen, IEquatable<BackgroundScreen>
    {
        public const float TRANSITION_LENGTH = 500;

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

        public override void OnEntering(ScreenTransitionEvent e)
        {
            if (animateOnEnter)
            {
                this.FadeOut();
                this.MoveToX(x_movement_amount);

                this.FadeIn(TRANSITION_LENGTH, Easing.InOutQuart);
                this.MoveToX(0, TRANSITION_LENGTH, Easing.InOutQuart);
            }

            base.OnEntering(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.MoveToX(-x_movement_amount, TRANSITION_LENGTH, Easing.InOutQuart);
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (IsLoaded)
            {
                this.FadeOut(TRANSITION_LENGTH, Easing.OutExpo);
                this.MoveToX(x_movement_amount, TRANSITION_LENGTH, Easing.OutExpo);
            }

            return base.OnExiting(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            if (IsLoaded)
                this.MoveToX(0, TRANSITION_LENGTH, Easing.OutExpo);
            base.OnResuming(e);
        }
    }
}
