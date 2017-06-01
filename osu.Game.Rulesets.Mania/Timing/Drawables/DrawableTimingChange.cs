using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public abstract class DrawableTimingChange : Container<DrawableHitObject>
    {
        protected readonly TimingChange TimingChange;

        protected override Container<DrawableHitObject> Content => content;
        private readonly Container<DrawableHitObject> content;

        public DrawableTimingChange(TimingChange timingChange)
        {
            TimingChange = timingChange;

            RelativeSizeAxes = Axes.Both;

            AddInternal(content = new RelativeCoordinateAutoSizingContainer
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                Y = (float)timingChange.Time
            });
        }

        protected override void Update()
        {
            var parent = (TimingChangeContainer)Parent;

            // Adjust our height to account for the speed changes
            Height = (float)(1000 / TimingChange.BeatLength / TimingChange.SpeedMultiplier);
            RelativeCoordinateSpace = new Vector2(1, (float)parent.TimeSpan);
        }

        public override void Add(DrawableHitObject drawable)
        {
            // The previously relatively-positioned drawable will now become relative to content, but since the drawable has no knowledge of content,
            // we need to offset it back by content's position position so that it becomes correctly relatively-positioned to content
            // This can be removed if hit objects were stored such that either their StartTime or their "beat offset" was relative to the timing change
            // they belonged to, but this requires a radical change to the beatmap format which we're not ready to do just yet
            drawable.Y -= (float)TimingChange.Time;

            base.Add(drawable);
        }

        /// <summary>
        /// Whether this timing change can contain a drawable. This is true if the drawable occurs "after" after this timing change.
        /// </summary>
        public bool CanContain(DrawableHitObject hitObject) => TimingChange.Time <= hitObject.HitObject.StartTime;

        private class RelativeCoordinateAutoSizingContainer : Container<DrawableHitObject>
        {
            protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

            public override void InvalidateFromChild(Invalidation invalidation)
            {
                // We only want to re-compute our size when a child's size or position has changed
                if ((invalidation & Invalidation.Geometry) == 0)
                {
                    base.InvalidateFromChild(invalidation);
                    return;
                }

                if (!Children.Any())
                    return;

                float height = Children.Select(child => child.Y + child.Height).Max();

                Height = height;
                RelativeCoordinateSpace = new Vector2(1, height);

                base.InvalidateFromChild(invalidation);
            }
        }
    }
}