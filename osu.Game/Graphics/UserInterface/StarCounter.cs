//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        protected float MinStarSize = 0.001f;

        protected FlowContainer starContainer;
        protected List<TextAwesome> stars = new List<TextAwesome>();

        public int MaxStars = 10;

        public StarCounter() : base()
        {
            RollingDuration = 5000;
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
                starContainer = new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };

            for (int i = 0; i < MaxStars; i++)
            {
                TextAwesome star = new TextAwesome
                {
                    Icon = FontAwesome.star,
                    Origin = Anchor.Centre,
                    TextSize = 20,
                };
                stars.Add(star);
                starContainer.Add(star);
            }

            // HACK: To mantain container height constant
            starContainer.Add(new TextAwesome
            {
                Icon = FontAwesome.star,
                Origin = Anchor.Centre,
                TextSize = 20,
                Alpha = 0.002f,
            });

            ResetCount();
        }

        protected override void transformVisibleCount(float currentValue, float newValue)
        {
            for (int i = 0; i < MaxStars; i++)
            {
                if (newValue < i)
                    stars[i].ScaleTo(MinStarSize);
                else if (newValue > (i + 1))
                    stars[i].ScaleTo(1f);
                else
                    stars[i].ScaleTo(Interpolation.ValueAt(newValue, MinStarSize, 1f, i, i + 1, EasingTypes.None));
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
