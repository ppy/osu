// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class SongProgressBar : CompositeDrawable
    {
        /// <summary>
        /// The current seek position of the audio, on a (0..1) range.
        /// This is generally the seek target, which will eventually match the gameplay clock when it catches up.
        /// </summary>
        protected double AudioProgress => length == 0 ? 1 : AudioTime / length;

        /// <summary>
        /// The current (non-frame-stable) audio time.
        /// </summary>
        protected double AudioTime => Math.Clamp(GameplayClock.CurrentTime - StartTime, 0.0, length);

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        /// <summary>
        /// Action which is invoked when a seek is requested, with the proposed millisecond value for the seek operation.
        /// </summary>
        public Action<double>? OnSeek { get; set; }

        /// <summary>
        /// Whether the progress bar should allow interaction, ie. to perform seek operations.
        /// </summary>
        public bool Interactive
        {
            get => InteractiveBindable.Value;
            set => InteractiveBindable.Value = value;
        }

        protected readonly BindableBool InteractiveBindable = new BindableBool();

        public double StartTime { get; set; }

        public double EndTime { get; set; } = 1.0;

        private double length => EndTime - StartTime;

        private bool handleClick;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            handleClick = true;
            return base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (handleClick)
                handleMouseInput(e);

            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            handleMouseInput(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            Vector2 posDiff = e.MouseDownPosition - e.MousePosition;

            if (Math.Abs(posDiff.X) < Math.Abs(posDiff.Y))
            {
                handleClick = false;
                return false;
            }

            handleMouseInput(e);
            return true;
        }

        private void handleMouseInput(UIEvent e)
        {
            if (!Interactive)
                return;

            double relativeX = Math.Clamp(ToLocalSpace(e.ScreenSpaceMousePosition).X / DrawWidth, 0, 1);
            onUserChange(StartTime + (EndTime - StartTime) * relativeX);
        }

        private ScheduledDelegate? scheduledSeek;

        private void onUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() => OnSeek?.Invoke(value));
        }
    }
}
