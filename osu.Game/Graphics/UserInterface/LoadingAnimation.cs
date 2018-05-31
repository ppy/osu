// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class LoadingAnimation : VisibilityContainer
    {
        private readonly SpriteIcon spinner;

        public LoadingAnimation()
        {
            Size = new Vector2(20);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                spinner = new SpriteIcon
                {
                    Size = new Vector2(20),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_spinner
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            spinner.Delay(0).ActuallySpinForever(2000, RotationDirection.Clockwise);
        }

        private const float transition_duration = 500;

        protected override void PopIn() => this.FadeIn(transition_duration * 5, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(transition_duration, Easing.OutQuint);
    }

    /// <summary>
    /// Temporary addition until https://github.com/ppy/osu-framework/issues/1520 is resolved.
    /// </summary>
    public static class FixedSpinExtensions
    {
        public static TransformSequence<T> ActuallySpinForever<T>(this TransformSequence<T> t, double revolutionDuration, RotationDirection direction, float startRotation = 0)
            where T : Drawable
        {
            var sequence = t.Spin(revolutionDuration, direction, startRotation, 1);

            sequence.OnComplete(o =>
            {
                using (o.BeginAbsoluteSequence(sequence.EndTime))
                    o.Spin(revolutionDuration, direction, startRotation);
            });

            return sequence;
        }

        public static TransformSequence<T> Spin<T>(this TransformSequence<T> t, double revolutionDuration, RotationDirection direction, float startRotation, int numRevolutions)
            where T : Drawable
        {
            if (numRevolutions < 1)
                throw new InvalidOperationException($"May not {nameof(Spin)} for fewer than 1 revolutions ({numRevolutions} attempted).");

            float endRotation = startRotation + (direction == RotationDirection.Clockwise ? 360 : -360);
            return t.RotateTo(startRotation).RotateTo(endRotation * numRevolutions, revolutionDuration * numRevolutions);
        }
    }
}
