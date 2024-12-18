// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public interface IGameplayClock : IFrameBasedClock
    {
        /// <summary>
        /// The time from which the clock should start. Will be seeked to on calling <see cref="GameplayClockContainer.Reset"/>.
        /// </summary>
        /// <remarks>
        /// By default, a value of zero will be used.
        /// Importantly, the value will be inferred from the current beatmap in <see cref="MasterGameplayClockContainer"/> by default.
        /// </remarks>
        double StartTime { get; }

        /// <summary>
        /// All adjustments applied to this clock which come from mods.
        /// </summary>
        IAdjustableAudioComponent AdjustmentsFromMods { get; }

        /// <summary>
        /// Whether gameplay is paused.
        /// </summary>
        IBindable<bool> IsPaused { get; }

        /// <summary>
        /// Whether the clock is currently rewinding.
        /// </summary>
        bool IsRewinding { get; }
    }
}
