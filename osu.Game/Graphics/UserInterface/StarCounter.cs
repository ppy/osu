// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public partial class StarCounter : Container
    {
        private readonly FillFlowContainer<Star> stars;

        /// <summary>
        /// Maximum amount of stars displayed.
        /// </summary>
        /// <remarks>
        /// This does not limit the counter value, but the amount of stars displayed.
        /// </remarks>
        public int StarCount { get; }

        /// <summary>
        /// The added delay for each subsequent star to be animated.
        /// </summary>
        protected virtual double AnimationDelay => 80;

        private const float star_spacing = 4;

        public virtual FillDirection Direction
        {
            set => stars.Direction = value;
        }

        private float current;

        /// <summary>
        /// Amount of stars represented.
        /// </summary>
        public float Current
        {
            get => current;

            set
            {
                if (current == value) return;

                if (IsLoaded)
                    animate(value);
                current = value;
            }
        }

        /// <summary>
        /// Shows a float count as stars. Used as star difficulty display.
        /// </summary>
        /// <param name="starCount">Maximum amount of stars to display.</param>
        public StarCounter(int starCount = 10)
        {
            StarCount = Math.Max(starCount, 0);

            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                stars = new FillFlowContainer<Star>
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(star_spacing),
                    ChildrenEnumerable = Enumerable.Range(0, StarCount).Select(_ => CreateStar())
                }
            };
        }

        public virtual Star CreateStar() => new DefaultStar();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Animate initial state from zero.
            ReplayAnimation();
        }

        public void ResetCount()
        {
            current = 0;
            StopAnimation();
        }

        public void ReplayAnimation()
        {
            float t = current;
            ResetCount();
            Current = t;
        }

        public void StopAnimation()
        {
            animate(current);
            foreach (var star in stars.Children)
                star.FinishTransforms(true);
        }

        private float getStarScale(int i, float value) => i + 1 <= value ? 1.0f : Interpolation.ValueAt(value, 0, 1.0f, i, i + 1);

        private void animate(float newValue)
        {
            for (int i = 0; i < stars.Children.Count; i++)
            {
                var star = stars.Children[i];

                star.ClearTransforms(true);

                double delay = (current <= newValue ? Math.Max(i - current, 0) : Math.Max(current - 1 - i, 0)) * AnimationDelay;

                using (star.BeginDelayedSequence(delay))
                    star.DisplayAt(getStarScale(i, newValue));
            }
        }

        public partial class DefaultStar : Star
        {
            private const double scaling_duration = 1000;

            private const double fading_duration = 100;

            private const Easing scaling_easing = Easing.OutElasticHalf;

            private const float min_star_scale = 0.4f;

            private const float star_size = 20;

            public readonly SpriteIcon Icon;

            public DefaultStar()
            {
                Size = new Vector2(star_size);

                InternalChild = Icon = new SpriteIcon
                {
                    Size = new Vector2(star_size),
                    Icon = FontAwesome.Solid.Star,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }

            public override void DisplayAt(float scale)
            {
                scale = (float)Interpolation.Lerp(min_star_scale, 1, Math.Clamp(scale, 0, 1));

                this.FadeTo(scale, fading_duration);
                Icon.ScaleTo(scale, scaling_duration, scaling_easing);
            }
        }

        public abstract partial class Star : CompositeDrawable
        {
            public abstract void DisplayAt(float scale);
        }
    }
}
