//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ComboCounter : AutoSizeContainer
    {
        protected Type transformType => typeof(TransformCombo);

        private bool rollbacking = false;

        protected ulong rollingTotalDuration = 0;

        /// <summary>
        /// If true, the roll-down duration will be proportional to the counter.
        /// </summary>
        public bool IsRollingProportional = true;

        /// <summary>
        /// If IsRollingProportional = false, duration in milliseconds for the counter roll-up animation for each
        /// element; else duration in milliseconds for the counter roll-up animation in total.
        /// </summary>
        public ulong RollingDuration = 0;

        /// <summary>
        /// Easing for the counter rollover animation.
        /// </summary>
        public EasingTypes RollingEasing = EasingTypes.None;

        protected ulong prevVisibleCount;
        protected ulong visibleCount;

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual ulong VisibleCount
        {
            get
            {
                return visibleCount;
            }
            protected set
            {
                if (visibleCount.Equals(value))
                    return;
                prevVisibleCount = visibleCount;
                visibleCount = value;
                transformVisibleCount(prevVisibleCount, visibleCount);
            }
        }

        protected ulong prevPrevCount;
        protected ulong prevCount;
        protected ulong count;

        /// <summary>
        /// Actual value of counter.
        /// </summary>
        public virtual ulong Count
        {
            get
            {
                return count;
            }
            set
            {
                prevPrevCount = prevCount;
                prevCount = count;
                count = value;
                if (IsLoaded)
                {
                    rollingTotalDuration =
                        IsRollingProportional
                            ? getProportionalDuration(VisibleCount, value)
                            : RollingDuration;
                    transformCount(VisibleCount, prevPrevCount, prevCount, value);
                }
            }
        }

        protected SpriteText countSpriteText;

        protected float textSize = 20.0f;
        public float TextSize
        {
            get { return textSize; }
            set
            {
                textSize = value;
                updateTextSize();
            }
        }

        /// <summary>
        /// Base of all combo counters.
        /// </summary>
        protected ComboCounter()
        {
            Children = new Drawable[]
            {
                countSpriteText = new SpriteText
                {
                    Anchor = this.Anchor,
                    Origin = this.Origin,
                    Alpha = 0,
                },
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            countSpriteText.Anchor = this.Anchor;
            countSpriteText.Origin = this.Origin;

            StopRolling();
        }

        /// <summary>
        /// Sets count value, bypassing rollover animation.
        /// </summary>
        /// <param name="count">New count value.</param>
        public virtual void SetCountWithoutRolling(ulong count)
        {
            Count = count;
            StopRolling();
        }

        /// <summary>
        /// Stops rollover animation, forcing the visible count to be the actual count.
        /// </summary>
        public virtual void StopRolling()
        {
            removeComboTransforms();
            VisibleCount = Count;
        }

        /// <summary>
        /// Animates roll-back to an specific value.
        /// </summary>
        /// <param name="newValue">Target value.</param>
        public virtual void RollBack(ulong newValue = 0)
        {
            rollbacking = true;
            Count = newValue;
            rollbacking = false;
        }

        /// <summary>
        /// Resets count to default value.
        /// </summary>
        public virtual void ResetCount()
        {
            SetCountWithoutRolling(default(ulong));
        }

        protected virtual ulong getProportionalDuration(ulong currentValue, ulong newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected abstract void transformVisibleCount(ulong currentValue, ulong newValue);

        protected virtual string formatCount(ulong count)
        {
            return count.ToString();
        }

        private void updateComboTransforms()
        {
            foreach (ITransform t in Transforms.AliveItems)
                if (t.GetType() == typeof(TransformCombo))
                    t.Apply(this);
        }

        private void removeComboTransforms()
        {
            Transforms.RemoveAll(t => t.GetType() == typeof(TransformCombo));
        }

        protected virtual void transformCount(ulong visibleValue, ulong prevValue, ulong currentValue, ulong newValue)
        {
            if (!rollbacking)
            {
                updateComboTransforms();
                removeComboTransforms();

                // If was decreasing, stops roll before increasing
                if (currentValue < prevValue)
                    VisibleCount = currentValue;

                VisibleCount = newValue;
            }
            else
            {
                transformCount(new TransformCombo(Clock), visibleValue, newValue);
            }
        }

        /// <summary>
        /// Intended to be used by transformCount().
        /// </summary>
        /// <see cref="transformCount"/>
        protected void transformCount(TransformCombo transform, ulong currentValue, ulong newValue)
        {
            updateComboTransforms();
            removeComboTransforms();

            if (Clock == null)
                return;

            if (RollingDuration == 0)
            {
                VisibleCount = Count;
                return;
            }

            transform.StartTime = Time;
            transform.EndTime = Time + rollingTotalDuration;
            transform.StartValue = currentValue;
            transform.EndValue = newValue;
            transform.Easing = RollingEasing;

            Transforms.Add(transform);
        }

        protected virtual void updateTextSize()
        {
            countSpriteText.TextSize = TextSize;
        }

        protected class TransformCombo : Transform<ulong>
        {
            public override ulong CurrentValue
            {
                get
                {
                    double time = Time;
                    if (time < StartTime) return StartValue;
                    if (time >= EndTime) return EndValue;

                    return (ulong)Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                (d as ComboCounter).VisibleCount = CurrentValue;
            }

            public TransformCombo(IClock clock)
                : base(clock)
            {
            }
        }
    }
}
