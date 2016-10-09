//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
    /// <summary>
    /// Shows a float count as stars. Used as star difficulty display.
    /// </summary>
    public class StarCounter : RollingCounter<float>
    {
        protected override Type transformType => typeof(TransformStar);

        protected float MinStarSize = 0.3f;

        protected Container starContainer;
        protected List<TextAwesome> stars = new List<TextAwesome>();

        public float MinStarAlpha = 0.5f;

        public int MaxStars = 10;

        public int StarSize = 20;

        public int StarSpacing = 4;

        public StarCounter() : base()
        {
            IsRollingProportional = true;
            RollingDuration = 150;
            RollingEasing = EasingTypes.Out;
        }

        protected override ulong getProportionalDuration(float currentValue, float newValue)
        {
            return (ulong)(Math.Abs(currentValue - newValue) * RollingDuration);
        }

        public override void ResetCount()
        {
            Count = 0;
            StopRolling();
        }

        public override void Load()
        {
            base.Load();

            Children = new Drawable[]
            {
                starContainer = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = MaxStars * StarSize,
                    Height = StarSize,
                }
            };

            for (int i = 0; i < MaxStars; i++)
            {
                TextAwesome star = new TextAwesome
                {
                    Icon = FontAwesome.star,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    TextSize = StarSize,
                    Scale = new Vector2(MinStarSize),
                    Alpha = MinStarAlpha,
                    Position = new Vector2((StarSize + StarSpacing) * i + (StarSize + StarSpacing) / 2, 0),
                };
                stars.Add(star);
                starContainer.Add(star);
            }

            ResetCount();
        }

        protected override void transformVisibleCount(float currentValue, float newValue)
        {
            for (int i = 0; i < MaxStars; i++)
            {
                if (newValue < i)
                {
                    stars[i].Alpha = MinStarAlpha;
                    stars[i].ScaleTo(MinStarSize);
                }
                else
                {
                    stars[i].Alpha = 1;
                    if (newValue > (i + 1))
                        stars[i].ScaleTo(1f);
                    else
                        stars[i].ScaleTo(Interpolation.ValueAt(newValue, MinStarSize, 1f, i, i + 1, EasingTypes.None));
                }
            }
        }

        protected class TransformStar : Transform<float>
        {
            public override float CurrentValue
            {
                get
                {
                    double time = Time;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as StarCounter).VisibleCount = CurrentValue;
            }

            public TransformStar(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
