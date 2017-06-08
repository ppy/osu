// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing.Drawables
{
    /// <summary>
    /// A container for hit objects which applies applies the speed changes defined by the <see cref="Timing.TimingSection.BeatLength"/> and <see cref="Timing.TimingSection.SpeedMultiplier"/>
    /// properties to its <see cref="Container{T}.Content"/> to affect the <see cref="HitObjectCollection"/> scroll speed.
    /// </summary>
    public abstract class DrawableTimingSection : Container<DrawableHitObject>
    {
        public readonly TimingSection TimingSection;

        protected override Container<DrawableHitObject> Content => content;
        private readonly Container<DrawableHitObject> content;

        private readonly Axes scrollingAxes;

        /// <summary>
        /// Creates a new <see cref="DrawableTimingSection"/>.
        /// </summary>
        /// <param name="timingSection">The encapsulated timing section that provides the speed changes.</param>
        /// <param name="scrollingAxes">The axes through which this drawable timing section scrolls through.</param>
        protected DrawableTimingSection(TimingSection timingSection, Axes scrollingAxes)
        {
            this.scrollingAxes = scrollingAxes;

            TimingSection = timingSection;

            AddInternal(content = CreateHitObjectCollection(scrollingAxes));
            content.RelativeChildOffset = new Vector2((scrollingAxes & Axes.X) > 0 ? (float)TimingSection.Time : 0, (scrollingAxes & Axes.Y) > 0 ? (float)TimingSection.Time : 0);
        }

        public override Axes RelativeSizeAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($"{nameof(DrawableTimingSection)} must always be relatively-sized."); }
        }

        protected override void Update()
        {
            var parent = Parent as Container;

            if (parent == null)
                return;

            float speedAdjustedSize = (float)(1000 / TimingSection.BeatLength / TimingSection.SpeedMultiplier);

            // The application of speed changes happens by modifying our size while maintaining the parent's relative child size as our own
            // By doing this the scroll speed of the hit objects is changed by a factor of Size / RelativeChildSize
            Size = new Vector2((scrollingAxes & Axes.X) > 0 ? speedAdjustedSize : 1, (scrollingAxes & Axes.Y) > 0 ? speedAdjustedSize : 1);
            RelativeChildSize = parent.RelativeChildSize;
        }

        /// <summary>
        /// Whether this timing change can contain a hit object. This is true if the hit object occurs after this timing change with respect to time.
        /// </summary>
        public bool CanContain(DrawableHitObject hitObject) => TimingSection.Time <= hitObject.HitObject.StartTime;

        /// <summary>
        /// Creates the container which handles the movement of a collection of hit objects.
        /// </summary>
        /// <param name="autoSizingAxes"></param>
        /// <returns></returns>
        protected abstract HitObjectCollection CreateHitObjectCollection(Axes autoSizingAxes);
    }
}