// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class ScrollingTimelineContainer : ScrollContainer
    {
        public readonly Bindable<bool> HitObjectsVisible = new Bindable<bool>();
        public readonly Bindable<bool> HitSoundsVisible = new Bindable<bool>();
        public readonly Bindable<bool> WaveformVisible = new Bindable<bool>();

        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        private readonly BeatmapWaveformGraph waveform;

        public ScrollingTimelineContainer()
            : base(Direction.Horizontal)
        {
            Masking = true;

            Add(waveform = new BeatmapWaveformGraph
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.FromHex("222"),
                Depth = float.MaxValue
            });

            Content.AutoSizeAxes = Axes.None;
            Content.RelativeSizeAxes = Axes.Both;

            WaveformVisible.ValueChanged += waveformVisibilityChanged;

            Zoom = 10;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap)
        {
            this.beatmap.BindTo(beatmap);
            this.beatmap.BindValueChanged(beatmapChanged, true);
        }

        private void beatmapChanged(WorkingBeatmap beatmap) => waveform.Beatmap = beatmap;

        private float minZoom = 1;
        /// <summary>
        /// The minimum zoom level allowed.
        /// </summary>
        public float MinZoom
        {
            get { return minZoom; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (minZoom == value)
                    return;
                minZoom = value;

                // Update the zoom level
                Zoom = Zoom;
            }
        }

        private float maxZoom = 30;
        /// <summary>
        /// The maximum zoom level allowed.
        /// </summary>
        public float MaxZoom
        {
            get { return maxZoom; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (maxZoom == value)
                    return;
                maxZoom = value;

                // Update the zoom level
                Zoom = Zoom;
            }
        }

        private float zoom = 1;
        /// <summary>
        /// The current zoom level.
        /// </summary>
        public float Zoom
        {
            get { return zoom; }
            set
            {
                value = MathHelper.Clamp(value, MinZoom, MaxZoom);
                if (zoom == value)
                    return;
                zoom = value;

                // Make the zoom target default to the center of the graph if it hasn't been set
                if (relativeContentZoomTarget == null)
                    relativeContentZoomTarget = ToSpaceOfOtherDrawable(DrawSize / 2, Content).X / Content.DrawSize.X;
                if (localZoomTarget == null)
                    localZoomTarget = DrawSize.X / 2;

                Content.ResizeWidthTo(Zoom);

                // Update the scroll position to focus on the zoom target
                float scrollPos = Content.DrawSize.X * relativeContentZoomTarget.Value - localZoomTarget.Value;
                ScrollTo(scrollPos, false);

                relativeContentZoomTarget = null;
                localZoomTarget = null;
            }
        }

        /// <summary>
        /// Zoom target as a relative position in the <see cref="ScrollingTimelineContainer.Content"/> space.
        /// </summary>
        private float? relativeContentZoomTarget;

        /// <summary>
        /// Zoom target as a position in our local space.
        /// </summary>
        private float? localZoomTarget;

        protected override bool OnScroll(InputState state)
        {
            if (!state.Keyboard.ControlPressed)
                return base.OnScroll(state);

            relativeContentZoomTarget = Content.ToLocalSpace(state.Mouse.NativeState.Position).X / Content.DrawSize.X;
            localZoomTarget = ToLocalSpace(state.Mouse.NativeState.Position).X;

            Zoom += state.Mouse.ScrollDelta.Y;

            return true;
        }

        private void waveformVisibilityChanged(bool visible) => waveform.FadeTo(visible ? 1 : 0, 200, Easing.OutQuint);
    }
}
