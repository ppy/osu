// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    /// <summary>
    /// A <see cref="SliderBody"/> which changes its curve depending on the snaking progress.
    /// </summary>
    public class SnakingSliderBody : SliderBody, ISliderProgress
    {
        public readonly List<Vector2> CurrentCurve = new List<Vector2>();

        public readonly Bindable<bool> SnakingIn = new Bindable<bool>();
        public readonly Bindable<bool> SnakingOut = new Bindable<bool>();

        public double? SnakedStart { get; private set; }
        public double? SnakedEnd { get; private set; }

        public override float PathRadius
        {
            get => base.PathRadius;
            set
            {
                if (base.PathRadius == value)
                    return;

                base.PathRadius = value;

                Refresh();
            }
        }

        public override Vector2 PathOffset => snakedPathOffset;

        /// <summary>
        /// The top-left position of the path when fully snaked.
        /// </summary>
        private Vector2 snakedPosition;

        /// <summary>
        /// The offset of the path from <see cref="snakedPosition"/> when fully snaked.
        /// </summary>
        private Vector2 snakedPathOffset;

        private readonly Slider slider;

        public SnakingSliderBody(Slider slider)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Refresh();
        }

        public void UpdateProgress(double completionProgress)
        {
            var span = slider.SpanAt(completionProgress);
            var spanProgress = slider.ProgressAt(completionProgress);

            double start = 0;
            double end = SnakingIn.Value ? MathHelper.Clamp((Time.Current - (slider.StartTime - slider.TimePreempt)) / (slider.TimePreempt / 3), 0, 1) : 1;

            if (span >= slider.SpanCount() - 1)
            {
                if (Math.Min(span, slider.SpanCount() - 1) % 2 == 1)
                {
                    start = 0;
                    end = SnakingOut.Value ? spanProgress : 1;
                }
                else
                {
                    start = SnakingOut.Value ? spanProgress : 0;
                }
            }

            setRange(start, end);
        }

        public void Refresh()
        {
            // Generate the entire curve
            slider.Path.GetPathToProgress(CurrentCurve, 0, 1);
            SetVertices(CurrentCurve);

            // Force the body to be the final path size to avoid excessive autosize computations
            Path.AutoSizeAxes = Axes.Both;
            Size = Path.Size;

            updatePathSize();

            snakedPosition = Path.PositionInBoundingBox(Vector2.Zero);
            snakedPathOffset = Path.PositionInBoundingBox(Path.Vertices[0]);

            var lastSnakedStart = SnakedStart ?? 0;
            var lastSnakedEnd = SnakedEnd ?? 0;

            SnakedStart = null;
            SnakedEnd = null;

            setRange(lastSnakedStart, lastSnakedEnd);
        }

        public override void RecyclePath()
        {
            base.RecyclePath();
            updatePathSize();
        }

        private void updatePathSize()
        {
            // Force the path to its final size to avoid excessive framebuffer resizes
            Path.AutoSizeAxes = Axes.None;
            Path.Size = Size;
        }

        private void setRange(double p0, double p1)
        {
            if (p0 > p1)
                MathHelper.Swap(ref p0, ref p1);

            if (SnakedStart == p0 && SnakedEnd == p1) return;

            SnakedStart = p0;
            SnakedEnd = p1;

            slider.Path.GetPathToProgress(CurrentCurve, p0, p1);

            SetVertices(CurrentCurve);

            // The bounding box of the path expands as it snakes, which in turn shifts the position of the path.
            // Depending on the direction of expansion, it may appear as if the path is expanding towards the position of the slider
            // rather than expanding out from the position of the slider.
            // To remove this effect, the path's position is shifted towards its final snaked position

            Path.Position = snakedPosition - Path.PositionInBoundingBox(Vector2.Zero);
        }
    }
}
