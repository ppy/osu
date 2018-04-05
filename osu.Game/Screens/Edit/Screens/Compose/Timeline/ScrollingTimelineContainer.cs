// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class ScrollingTimelineContainer : ScrollContainer
    {
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private readonly Container waveformContainer;

        private float currentZoom = 10;

        public ScrollingTimelineContainer()
            : base(Direction.Horizontal)
        {
            Masking = true;

            BeatmapWaveformGraph waveform;
            Child = waveformContainer = new Container
            {
                RelativeSizeAxes = Axes.Y,
                Child = waveform = new BeatmapWaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("222"),
                    Depth = float.MaxValue
                }
            };

            waveform.Beatmap.BindTo(Beatmap);

            WaveformVisible.ValueChanged += visible => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            waveformContainer.Margin = new MarginPadding { Horizontal = DrawWidth / 2 };
            waveformContainer.Width = DrawWidth * currentZoom;
        }

        protected override bool OnWheel(InputState state)
        {
            if (!state.Keyboard.ControlPressed)
                return base.OnWheel(state);

            setZoomTarget(zoomTarget + state.Mouse.WheelDelta, waveformContainer.ToLocalSpace(state.Mouse.NativeState.Position).X);
            return true;
        }

        private int zoomTarget = 10;
        private void setZoomTarget(int newZoom, float focusPoint)
        {
            zoomTarget = MathHelper.Clamp(newZoom, 1, 60);
            transformZoomTo(zoomTarget, focusPoint, 200, Easing.OutQuint);
        }

        private void transformZoomTo(int newZoom, float focusPoint, double duration = 0, Easing easing = Easing.None)
            => this.TransformTo(this.PopulateTransform(new TransformZoom(focusPoint, waveformContainer.DrawWidth, DrawWidth, Current), newZoom, duration, easing));

        private class TransformZoom : Transform<float, ScrollingTimelineContainer>
        {
            private readonly float focusPoint;
            private readonly float focusSize;
            private readonly float sizeReference;
            private readonly float scrollOffset;

            public TransformZoom(float focusPoint, float focusSize, float sizeReference, float scrollOffset)
            {
                this.focusPoint = focusPoint;
                this.focusSize = focusSize;
                this.sizeReference = sizeReference;
                this.scrollOffset = scrollOffset;
            }

            public override string TargetMember => nameof(currentZoom);

            private float valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            protected override void Apply(ScrollingTimelineContainer d, double time)
            {
                float newZoom = valueAt(time);

                float focusOffset = focusPoint - scrollOffset;
                float expectedWidth = sizeReference * newZoom;
                float targetOffset = expectedWidth * (focusPoint / focusSize) - focusOffset;

                d.currentZoom = newZoom;
                d.ScrollTo(targetOffset, false);
            }

            protected override void ReadIntoStartValue(ScrollingTimelineContainer d) => StartValue = d.currentZoom;
        }
    }
}
