// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Handles debouncing hover sounds at a global level to ensure the effects are not overwhelming.
    /// </summary>
    public abstract partial class HoverSampleDebounceComponent : Component
    {
        private Bindable<double?> lastPlaybackTime;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.ReceivePositionalInputAt(screenSpacePos) == true;

        [BackgroundDependencyLoader]
        private void load(SessionStatics statics)
        {
            lastPlaybackTime = statics.GetBindable<double?>(Static.LastHoverSoundPlaybackTime);
        }

        protected override bool OnHover(HoverEvent e)
        {
            // hover sounds shouldn't be played during scroll operations.
            if (e.HasAnyButtonPressed)
                return false;

            bool enoughTimePassedSinceLastPlayback = !lastPlaybackTime.Value.HasValue || Time.Current - lastPlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

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
