// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Audio
{
    /// <summary>
    /// Handles volume changes to the global music track.
    /// </summary>
    public class BackgroundTrackManager : Component
    {
        private readonly BindableDouble muteBindable = new BindableDouble(1);

        private readonly BindableDouble dimBindable = new BindableDouble(1);

        [Resolved]
        private AudioManager audio { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            audio.Tracks.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            audio.Tracks.AddAdjustment(AdjustableProperty.Volume, dimBindable);
        }

        /// <summary>
        /// Mutes the music track.
        /// </summary>
        /// <param name="duration">The duration of the volume transformation.</param>
        /// <param name="easing">The easing of the volume transformation.</param>
        public void Mute(double duration = 0D, Easing easing = Easing.None)
        {
            this.TransformBindableTo(muteBindable, 0, duration, easing);
        }

        /// <summary>
        /// Unmutes the music track.
        /// </summary>
        /// <param name="duration">The duration of the volume transformation.</param>
        /// <param name="easing">The easing of the volume transformation.</param>
        public void Unmute(double duration = 0D, Easing easing = Easing.None)
        {
            this.TransformBindableTo(muteBindable, 1, duration, easing);
        }

        /// <summary>
        /// Dims the volume of the music track.
        /// </summary>
        /// <param name="volume">The volume adjustment level.</param>
        /// <param name="duration">The duration of the volume transformation.</param>
        /// <param name="easing">The easing of the volume transformation.</param>
        public void SetDimming(double volume, double duration = 0D, Easing easing = Easing.None)
        {
            this.TransformBindableTo(dimBindable, volume, duration, easing);
        }

        /// <summary>
        /// Removes any volume dimming from the music track.
        /// </summary>
        /// <param name="duration">The duration of the volume transformation.</param>
        /// <param name="easing">The easing of the volume transformation.</param>
        public void RemoveDimming(double duration = 0D, Easing easing = Easing.None)
        {
            SetDimming(1, duration, easing);
        }
    }
}
