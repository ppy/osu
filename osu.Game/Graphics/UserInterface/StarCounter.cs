// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class StarCounter : Container
    {
        private readonly Container<Star> stars;

        /// <summary>
        /// Maximum amount of stars displayed.
        /// </summary>
        /// <remarks>
        /// This does not limit the counter value, but the amount of stars displayed.
        /// </remarks>
        public int StarCount { get; }

        private double animationDelay => 80;

        private double scalingDuration => 1000;
        private Easing scalingEasing => Easing.OutElasticHalf;
        private float minStarScale => 0.4f;

        private double fadingDuration => 100;
        private float minStarAlpha => 0.5f;

        private const float star_size = 20;
        private const float star_spacing = 4;

        private float countStars;

        /// <summary>
        /// Amount of stars represented.
        /// </summary>
        public float CountStars
        {
            get => countStars;

            set
            {
                if (countStars == value) return;

                if (IsLoaded)
                    transformCount(value);
                countStars = value;
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
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(star_spacing),
                    ChildrenEnumerable = Enumerable.Range(0, StarCount).Select(i => new Star { Alpha = minStarAlpha })
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Animate initial state from zero.
            ReplayAnimation();
        }

        public void ResetCount()
        {
            countStars = 0;
            StopAnimation();
        }

        public void ReplayAnimation()
        {
            var t = countStars;
            ResetCount();
            CountStars = t;
        }

        public void StopAnimation()
        {
            int i = 0;

            foreach (var star in stars.Children)
            {
                star.ClearTransforms(true);
                star.FadeTo(i < countStars ? 1.0f : minStarAlpha);
                star.Icon.ScaleTo(getStarScale(i, countStars));
                i++;
            }
        }

        private float getStarScale(int i, float value)
        {
            if (value <= i)
                return minStarScale;

            return i + 1 <= value ? 1.0f : Interpolation.ValueAt(value, minStarScale, 1.0f, i, i + 1);
        }

        private void transformCount(float newValue)
        {
            int i = 0;

            foreach (var star in stars.Children)
            {
                star.ClearTransforms(true);

                var delay = (countStars <= newValue ? Math.Max(i - countStars, 0) : Math.Max(countStars - 1 - i, 0)) * animationDelay;
                star.Delay(delay).FadeTo(i < newValue ? 1.0f : minStarAlpha, fadingDuration);
                star.Icon.Delay(delay).ScaleTo(getStarScale(i, newValue), scalingDuration, scalingEasing);

                i++;
            }
        }

        private class Star : Container
        {
            public readonly SpriteIcon Icon;

            public Star()
            {
                Size = new Vector2(star_size);

                Child = Icon = new SpriteIcon
                {
                    Size = new Vector2(star_size),
                    Icon = FontAwesome.Solid.Star,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            }
        }
    }
}
