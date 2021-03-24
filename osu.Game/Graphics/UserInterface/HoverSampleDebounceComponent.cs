// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Handles debouncing hover sounds at a global level to ensure the effects are not overwhelming.
    /// </summary>
    public abstract class HoverSampleDebounceComponent : CompositeDrawable
    {
        /// <summary>
        /// Length of debounce for hover sound playback, in milliseconds.
        /// </summary>
        public double HoverDebounceTime { get; } = 20;

        private Bindable<double?> lastPlaybackTime;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SessionStatics statics)
        {
            lastPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);
        }

        protected override bool OnHover(HoverEvent e)
        {
            // hover sounds shouldn't be played during scroll operations.
            if (e.HasAnyButtonPressed)
                return false;

            bool enoughTimePassedSinceLastPlayback = !lastPlaybackTime.Value.HasValue || Time.Current - lastPlaybackTime.Value >= HoverDebounceTime;

            if (enoughTimePassedSinceLastPlayback)
            {
                PlayHoverSample();
                lastPlaybackTime.Value = Time.Current;
            }

            return false;
        }

        public abstract void PlayHoverSample();
    }
}
