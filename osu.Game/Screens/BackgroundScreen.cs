// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private readonly bool animateOnEnter;

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

        private const float transition_length = 500;
        private const float x_movement_amount = 50;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            //we don't want to handle escape key.
            return false;
        }

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

                this.FadeIn(transition_length, Easing.InOutQuart);
                this.MoveToX(0, transition_length, Easing.InOutQuart);
            }

            base.OnEntering(last);
        }

        public override void OnSuspending(IScreen next)
        {
            this.MoveToX(-x_movement_amount, transition_length, Easing.InOutQuart);
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(transition_length, Easing.OutExpo);
            this.MoveToX(x_movement_amount, transition_length, Easing.OutExpo);

            return base.OnExiting(next);
        }

        public override void OnResuming(IScreen last)
        {
            this.MoveToX(0, transition_length, Easing.OutExpo);
            base.OnResuming(last);
        }
    }
}
