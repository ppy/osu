// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public interface IGameplayClock : IFrameBasedClock
    {
        /// <summary>
        /// The rate of gameplay when playback is at 100%.
        /// This excludes any seeking / user adjustments.
        /// </summary>
        double TrueGameplayRate { get; }

        /// <summary>
        /// The time from which the clock should start. Will be seeked to on calling <see cref="GameplayClockContainer.Reset"/>.
        /// </summary>
        /// <remarks>
        /// If not set, a value of zero will be used.
        /// Importantly, the value will be inferred from the current ruleset in <see cref="MasterGameplayClockContainer"/> unless specified.
        /// </remarks>
        double? StartTime { get; }

        /// <summary>
        /// All adjustments applied to this clock which don't come from gameplay or mods.
        /// </summary>
        IEnumerable<double> NonGameplayAdjustments { get; }

        IBindable<bool> IsPaused { get; }
    }
}
