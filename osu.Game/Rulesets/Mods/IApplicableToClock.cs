﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make adjustments to the track.
    /// </summary>
    public interface IApplicableToClock : IApplicableMod
    {
        void ApplyToClock(IAdjustableClock clock);
    }
}
