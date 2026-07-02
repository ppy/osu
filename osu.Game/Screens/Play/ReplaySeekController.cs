// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Provides seek operations for replay playback, including smooth animated seeking.
    /// This is not a clock — <see cref="GameplayClockContainer"/> remains the timing source.
    /// This component exists solely to give <see cref="ReplayBookmarkController"/> a way to
    /// seek without a direct reference to <see cref="ReplayPlayer"/>.
    /// </summary>
    public partial class ReplaySeekController : Component
    {
        private readonly GameplayClockContainer gameplayClockContainer;

        public double CurrentTime => gameplayClockContainer.CurrentTime;

        public bool IsRunning => gameplayClockContainer.IsRunning;

        public double Rate => ((IAdjustableClock)gameplayClockContainer).Rate;

        public ReplaySeekController(GameplayClockContainer gameplayClockContainer)
        {
            this.gameplayClockContainer = gameplayClockContainer;
        }

        public void Seek(double time) => gameplayClockContainer.Seek(time);

        /// <summary>
        /// Seeks smoothly to the destination if close enough, otherwise seeks immediately.
        /// </summary>
        public void SeekSmoothlyTo(double seekDestination)
        {
            const double smooth_seek_max_proximity = 5000;

            if (IsRunning || Math.Abs(seekDestination - CurrentTime) > smooth_seek_max_proximity)
            {
                Seek(seekDestination);
                return;
            }

            transformSeekTo(seekDestination, 300, Easing.OutQuint);
        }

        private void transformSeekTo(double seek, double duration = 0, Easing easing = Easing.None)
            => this.TransformTo(this.PopulateTransform(new TransformSeek(), seek, duration, easing));

        private double currentTime
        {
            get => gameplayClockContainer.CurrentTime;
            set => gameplayClockContainer.Seek(value);
        }

        private class TransformSeek : Transform<double, ReplaySeekController>
        {
            public override string TargetMember => nameof(currentTime);

            protected override void Apply(ReplaySeekController controller, double time) => controller.currentTime = valueAt(time);

            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            protected override void ReadIntoStartValue(ReplaySeekController controller) => StartValue = controller.currentTime;
        }
    }
}
