// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.MathUtils;
using System;

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
        private EasingTypes scalingEasing => EasingTypes.OutElasticHalf;
        private float minStarScale => 0.4f;

        private double fadingDuration => 100;
        private float minStarAlpha => 0.5f;

        private const float star_size = 20;
        private const float star_spacing = 4;

        private float count;

        /// <summary>
        /// Amount of stars represented.
        /// </summary>
        public float Count
        {
            get
            {
                return count;
            }

            set
            {
                if (count == value) return;

                if (IsLoaded)
                    transformCount(value);
                count = value;
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
                }
            };

            for (int i = 0; i < StarCount; i++)
            {
                stars.Add(new Star
                {
                    Alpha = minStarAlpha,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Animate initial state from zero.
            ReplayAnimation();
        }

        public void ResetCount()
        {
            count = 0;
            StopAnimation();
        }

        public void ReplayAnimation()
        {
            var t = count;
            ResetCount();
            Count = t;
        }

        public void StopAnimation()
        {
            int i = 0;
            foreach (var star in stars.Children)
            {
                star.ClearTransforms(true);
                star.FadeTo(i < count ? 1.0f : minStarAlpha);
                star.Icon.ScaleTo(getStarScale(i, count));
                i++;
            }
        }

        private float getStarScale(int i, float value)
        {
            if (value <= i)
                return minStarScale;

            return i + 1 <= value ? 1.0f : (float)Interpolation.ValueAt(value, minStarScale, 1.0f, i, i + 1);
        }

        private void transformCount(float newValue)
        {
            int i = 0;
            foreach (var star in stars.Children)
            {
                star.ClearTransforms(true);
                if (count <= newValue)
                    star.Delay(Math.Max(i - count, 0) * animationDelay, true);
                else
                    star.Delay(Math.Max(count - 1 - i, 0) * animationDelay, true);

                star.FadeTo(i < newValue ? 1.0f : minStarAlpha, fadingDuration);
                star.Icon.ScaleTo(getStarScale(i, newValue), scalingDuration, scalingEasing);
                star.DelayReset();

                i++;
            }
        }

        private class Star : Container
        {
            public readonly TextAwesome Icon;
            public Star()
            {
                Size = new Vector2(star_size);

                Children = new[]
                {
                    Icon = new TextAwesome
                    {
                        TextSize = star_size,
                        Icon = FontAwesome.fa_star,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
            }
        }
    }
}
