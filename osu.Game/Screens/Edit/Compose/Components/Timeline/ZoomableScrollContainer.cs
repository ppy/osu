// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class ZoomableScrollContainer : OsuScrollContainer
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

        /// <summary>
        /// The current zoom level of <see cref="ZoomableScrollContainer" />.
        /// It may differ from <see cref="Zoom" /> during transitions.
        /// </summary>
        public float CurrentZoom => currentZoom;

        [Resolved(canBeNull: true)]
        private IFrameBasedClock editorClock { get; set; }

        public ZoomableScrollContainer()
            : base(Direction.Horizontal)
        {
            base.Content.Add(zoomedContent = new Container { RelativeSizeAxes = Axes.Y });
        }

        private float minZoom = 1;

        /// <summary>
        /// The minimum zoom level allowed.
        /// </summary>
        public float MinZoom
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

        private float maxZoom = 60;

        /// <summary>
        /// The maximum zoom level allowed.
        /// </summary>
        public float MaxZoom
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
                value = Math.Clamp(value, MinZoom, MaxZoom);

                if (IsLoaded)
                    setZoomTarget(value, ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), zoomedContent).X);
                else
                    currentZoom = zoomTarget = value;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // This width only gets updated on the application of a transform, so this needs to be initialized here.
            updateZoomedContentWidth();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.AltPressed)
            {
                // zoom when holding alt.
                setZoomTarget(zoomTarget + e.ScrollDelta.Y, zoomedContent.ToLocalSpace(e.ScreenSpaceMousePosition).X);
                return true;
            }

            // can't handle scroll correctly while playing.
            // the editor will handle this case for us.
            if (editorClock?.IsRunning == true)
                return false;

            return base.OnScroll(e);
        }

        private void updateZoomedContentWidth() => zoomedContent.Width = DrawWidth * currentZoom;

        private float zoomTarget = 1;

        private void setZoomTarget(float newZoom, float focusPoint)
        {
            zoomTarget = Math.Clamp(newZoom, MinZoom, MaxZoom);
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
            /// Transforms <see cref="ZoomableScrollContainer.currentZoom"/> to a new value.
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

                d.updateZoomedContentWidth();
                // Temporarily here to make sure ScrollTo gets the correct DrawSize for scrollable area.
                // TODO: Make sure draw size gets invalidated properly on the framework side, and remove this once it is.
                d.Invalidate(Invalidation.DrawSize);
                d.ScrollTo(targetOffset, false);
            }

            protected override void ReadIntoStartValue(ZoomableScrollContainer d) => StartValue = d.currentZoom;
        }
    }
}
