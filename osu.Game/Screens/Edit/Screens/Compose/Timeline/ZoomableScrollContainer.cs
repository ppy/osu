// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class ZoomableScrollContainer : ScrollContainer
    {
        private readonly Container zoomedContent;
        protected override Container<Drawable> Content => zoomedContent;

        private float currentZoom = 10;

        public ZoomableScrollContainer()
            : base(Direction.Horizontal)
        {
            base.Content.Add(zoomedContent = new Container { RelativeSizeAxes = Axes.Y });
        }

        /// <summary>
        /// Gets or sets the content zoom of this <see cref="Timeline"/>.
        /// </summary>
        public int Zoom
        {
            get => zoomTarget;
            set => setZoomTarget(value, ToSpaceOfOtherDrawable(new Vector2(DrawWidth / 2, 0), zoomedContent).X);
        }

        protected override void Update()
        {
            base.Update();

            zoomedContent.Width = DrawWidth * currentZoom;
        }

        protected override bool OnWheel(InputState state)
        {
            if (!state.Keyboard.ControlPressed)
                return base.OnWheel(state);

            setZoomTarget(zoomTarget + state.Mouse.WheelDelta, zoomedContent.ToLocalSpace(state.Mouse.NativeState.Position).X);
            return true;
        }

        private int zoomTarget = 10;
        private void setZoomTarget(int newZoom, float focusPoint)
        {
            zoomTarget = MathHelper.Clamp(newZoom, 1, 60);
            transformZoomTo(zoomTarget, focusPoint, 200, Easing.OutQuint);
        }

        private void transformZoomTo(int newZoom, float focusPoint, double duration = 0, Easing easing = Easing.None)
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
