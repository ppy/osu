﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class ZoomableScrollContainer : ScrollContainer
    {
        /// <summary>
        /// The time to zoom into/out of a point.
        /// All user scroll input will be overwritten during the zoom transform.
        /// </summary>
        public double ZoomDuration;

        /// <summary>
        /// The easing with which to transform the zoom.
        /// </summary>
        public Easing ZoomEasing;

        private readonly Container zoomedContent;
        protected override Container<Drawable> Content => zoomedContent;

        private float currentZoom = 1;

        public ZoomableScrollContainer()
            : base(Direction.Horizontal)
        {
            base.Content.Add(zoomedContent = new Container { RelativeSizeAxes = Axes.Y });
        }

        private int minZoom = 1;

        /// <summary>
        /// The minimum zoom level allowed.
        /// </summary>
        public int MinZoom
        {
            get => minZoom;
            set
            {
                if (value < 1)
                    throw new ArgumentException($"{nameof(MinZoom)} must be >= 1.", nameof(value));
                minZoom = value;

                if (Zoom < value)
                    Zoom = value;
            }
        }

        private int maxZoom = 60;

        /// <summary>
        /// The maximum zoom level allowed.
        /// </summary>
        public int MaxZoom
        {
            get => maxZoom;
            set
            {
                if (value < 1)
                    throw new ArgumentException($"{nameof(MaxZoom)} must be >= 1.", nameof(value));
                maxZoom = value;

                if (Zoom > value)
                    Zoom = value;
            }
        }

        /// <summary>
        /// Gets or sets the content zoom level of this <see cref="ZoomableScrollContainer"/>.
        /// </summary>
        public float Zoom
        {
            get => zoomTarget;
            set
            {
                value = MathHelper.Clamp(value, MinZoom, MaxZoom);

                if (IsLoaded)
                    setZoomTarget(value, ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), zoomedContent).X);
                else
                    currentZoom = zoomTarget = value;
            }
        }

        protected override void Update()
        {
            base.Update();

            zoomedContent.Width = DrawWidth * currentZoom;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.IsPrecise)
                // for now, we don't support zoom when using a precision scroll device. this needs gesture support.
                return base.OnScroll(e);

            setZoomTarget(zoomTarget + e.ScrollDelta.Y, zoomedContent.ToLocalSpace(e.ScreenSpaceMousePosition).X);
            return true;
        }

        private float zoomTarget = 1;
        private void setZoomTarget(float newZoom, float focusPoint)
        {
            zoomTarget = MathHelper.Clamp(newZoom, MinZoom, MaxZoom);
            transformZoomTo(zoomTarget, focusPoint, ZoomDuration, ZoomEasing);
        }

        private void transformZoomTo(float newZoom, float focusPoint, double duration = 0, Easing easing = Easing.None)
            => this.TransformTo(this.PopulateTransform(new TransformZoom(focusPoint, zoomedContent.DrawWidth, Current), newZoom, duration, easing));

        private class TransformZoom : Transform<float, ZoomableScrollContainer>
        {
            /// <summary>
            /// The focus point in absolute coordinates local to the content.
            /// </summary>
            private readonly float focusPoint;

            /// <summary>
            /// The size of the content.
            /// </summary>
            private readonly float contentSize;

            /// <summary>
            /// The scroll offset at the start of the transform.
            /// </summary>
            private readonly float scrollOffset;

            /// <summary>
            /// Transforms <see cref="TimeTimelinem"/> to a new value.
            /// </summary>
            /// <param name="focusPoint">The focus point in absolute coordinates local to the content.</param>
            /// <param name="contentSize">The size of the content.</param>
            /// <param name="scrollOffset">The scroll offset at the start of the transform.</param>
            public TransformZoom(float focusPoint, float contentSize, float scrollOffset)
            {
                this.focusPoint = focusPoint;
                this.contentSize = contentSize;
                this.scrollOffset = scrollOffset;
            }

            public override string TargetMember => nameof(currentZoom);

            private float valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            protected override void Apply(ZoomableScrollContainer d, double time)
            {
                float newZoom = valueAt(time);

                float focusOffset = focusPoint - scrollOffset;
                float expectedWidth = d.DrawWidth * newZoom;
                float targetOffset = expectedWidth * (focusPoint / contentSize) - focusOffset;

                d.currentZoom = newZoom;
                d.ScrollTo(targetOffset, false);
            }

            protected override void ReadIntoStartValue(ZoomableScrollContainer d) => StartValue = d.currentZoom;
        }
    }
}
