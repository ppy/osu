//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class StarCounter : AutoSizeContainer
    {
        private Container starContainer;
        private List<TextAwesome> stars = new List<TextAwesome>();

        private double transformStartTime = 0;

        /// <summary>
        /// Maximum amount of stars displayed.
        /// </summary>
        /// <remarks>
        /// This does not limit the counter value, but the amount of stars displayed.
        /// </remarks>
        public int MaxStars
        {
            get;
            protected set;
        }

        public double AnimationDelay = 150;

        public double ScalingDuration = 500;
        public EasingTypes ScalingEasing = EasingTypes.OutElasticHalf;
        public float MinStarScale = 0.3f;

        public double FadingDuration = 100;
        public float MinStarAlpha = 0.5f;

        public float StarSize = 20;
        public float StarSpacing = 4;

        public float VisibleValue
        {
            get
            {
                double elapsedTime = Time - transformStartTime;
                double expectedElapsedTime = Math.Abs(prevCount - count) * AnimationDelay;
                if (elapsedTime >= expectedElapsedTime)
                    return count;
                return Interpolation.ValueAt(elapsedTime, prevCount, count, 0, expectedElapsedTime);
            }
        }

        private float prevCount;
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
                prevCount = VisibleValue;
                count = value;
                if (IsLoaded)
                {
                    transformCount(prevCount, count);
                }
            }
        }

        /// <summary>
        /// Shows a float count as stars. Used as star difficulty display.
        /// </summary>
        /// <param name="stars">Maximum amount of stars to display.</param>
        public StarCounter(int stars = 10)
        {
            MaxStars = Math.Max(stars, 0);

            Children = new Drawable[]
            {
                starContainer = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            starContainer.Width = MaxStars * StarSize + Math.Max(MaxStars - 1, 0) * StarSpacing;
            starContainer.Height = StarSize;

            for (int i = 0; i < MaxStars; i++)
            {
                TextAwesome star = new TextAwesome
                {
                    Icon = FontAwesome.star,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    TextSize = StarSize,
                    Position = new Vector2((StarSize + StarSpacing) * i + (StarSize + StarSpacing) / 2, 0),
                };

                //todo: user Container<T> once we have it.
                stars.Add(star);
                starContainer.Add(star);
            }

            StopAnimation();
        }

        public void ResetCount()
        {
            Count = 0;
            StopAnimation();
        }

        public void StopAnimation()
        {
            prevCount = count;
            transformStartTime = Time;

            for (int i = 0; i < MaxStars; i++)
            {
                stars[i].DelayReset();
                transformStarQuick(i, count);
            }
        }

        private float getStarScale(int i, float value)
        {
            if (value <= i)
                return MinStarScale;
            if (i + 1 <= value)
                return 1.0f;
            return Interpolation.ValueAt(value, MinStarScale, 1.0f, i, i + 1);
        }

        private void transformStar(int i, float value)
        {
            stars[i].FadeTo(i < value ? 1.0f : MinStarAlpha, FadingDuration);
            stars[i].ScaleTo(getStarScale(i, value), ScalingDuration, ScalingEasing);
        }

        private void transformStarQuick(int i, float value)
        {
            stars[i].FadeTo(i < value ? 1.0f : MinStarAlpha);
            stars[i].ScaleTo(getStarScale(i, value));
        }

        private void transformCount(float currentValue, float newValue)
        {
            if (currentValue < newValue)
            {
                for (int i = 0; i < MaxStars; i++)
                {
                    stars[i].DelayReset();
                    stars[i].ClearTransformations();
                    if (i > currentValue)
                        stars[i].Delay((i - currentValue) * AnimationDelay);
                    transformStar(i, newValue);
                }
            }
            else
            {
                for (int i = MaxStars - 1; i >= 0; i--)
                {
                    stars[i].DelayReset();
                    stars[i].ClearTransformations();
                    if (i < (currentValue - 1))
                        stars[i].Delay((currentValue - 1 - i) * AnimationDelay);
                    transformStar(i, newValue);
                }
            }
            transformStartTime = Time;
        }
    }
}
