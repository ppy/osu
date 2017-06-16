// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A collection of hit objects which scrolls within a <see cref="SpeedAdjustmentContainer"/>.
    ///
    /// <para>
    /// This container handles the conversion between time and position through <see cref="Container{T}.RelativeChildSize"/> and
    /// <see cref="Container{T}.RelativeChildOffset"/> such that hit objects added to this container should have time values set as their
    /// positions/sizes to make proper use of this container.
    /// </para>
    ///
    /// <para>
    /// This container will auto-size to the total duration of the contained hit objects along the desired auto-sizing axes such that the resulting size
    /// of this container will be a value representing the total duration of all contained hit objects.
    /// </para>
    ///
    /// <para>
    /// This container is and must always be relatively-sized and positioned to its such that the parent can utilise <see cref="Container{T}.RelativeChildSize"/>
    /// and <see cref="Container{T}.RelativeChildOffset"/> to apply further time offsets to this collection of hit objects.
    /// </para>
    /// </summary>
    public abstract class DrawableTimingSection : Container<DrawableHitObject>
    {
        private readonly BindableDouble visibleTimeRange = new BindableDouble();
        /// <summary>
        /// Gets or sets the range of time that is visible by the length of this container.
        /// </summary>
        public BindableDouble VisibleTimeRange
        {
            get { return visibleTimeRange; }
            set { visibleTimeRange.BindTo(value); }
        }

        protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

        /// <summary>
        /// Axes through which this timing section scrolls. This is set from <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        internal Axes ScrollingAxes;

        private Cached layout = new Cached();

        /// <summary>
        /// Creates a new <see cref="DrawableTimingSection"/>.
        /// </summary>
        protected DrawableTimingSection()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
        }

        public override void InvalidateFromChild(Invalidation invalidation)
        {
            // We only want to re-compute our size when a child's size or position has changed
            if ((invalidation & Invalidation.RequiredParentSizeToFit) == 0)
            {
                base.InvalidateFromChild(invalidation);
                return;
            }

            layout.Invalidate();

            base.InvalidateFromChild(invalidation);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!layout.EnsureValid())
            {
                layout.Refresh(() =>
                {
                    if (!Children.Any())
                        return;

                    //double maxDuration = Children.Select(c => (c.HitObject as IHasEndTime)?.EndTime ?? c.HitObject.StartTime).Max();
                    //float width = (float)maxDuration - RelativeChildOffset.X;
                    //float height = (float)maxDuration - RelativeChildOffset.Y;


                    // Auto-size to the total size of our children
                    // This ends up being the total duration of our children, however for now this is a more sure-fire way to calculate this
                    // than the above due to some undesired masking optimisations causing some hit objects to be culled...
                    // Todo: When this is investigated more we should use the above method as it is a little more exact
                    // Todo: This is not working correctly in the case that hit objects are absolutely-sized - needs a proper looking into in osu!framework
                    float width = Children.Select(child => child.X + child.Width).Max() - RelativeChildOffset.X;
                    float height = Children.Select(child => child.Y + child.Height).Max() - RelativeChildOffset.Y;

                    // Consider that width/height are time values. To have ourselves span these time values 1:1, we first need to set our size
                    Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? width : Size.X, (ScrollingAxes & Axes.Y) > 0 ? height : Size.Y);
                    // Then to make our position-space be time values again, we need our relative child size to follow our size
                    RelativeChildSize = Size;
                });
            }
        }
    }
}
