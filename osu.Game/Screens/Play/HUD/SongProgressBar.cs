// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Threading;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class SongProgressBar : SliderBar<double>
    {
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

        public double StartTime
        {
            get => CurrentNumber.MinValue;
            set => CurrentNumber.MinValue = value;
        }

        public double EndTime
        {
            get => CurrentNumber.MaxValue;
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            get => CurrentNumber.Value;
            set => CurrentNumber.Value = value;
        }

        protected SongProgressBar()
        {
            StartTime = 0;
            EndTime = 1;
        }

        protected override void UpdateValue(float value)
        {
            // handled in update
        }

        private ScheduledDelegate? scheduledSeek;

        protected override void OnUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (Interactive)
                    OnSeek?.Invoke(value);
            });
        }
    }
}
