// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that ends at a different time than its start time.
    /// </summary>
    [Obsolete("Use IHasDuration instead.")] // can be removed 20201126
    public interface IHasEndTime
    {
        /// <summary>
        /// The time at which the HitObject ends.
        /// </summary>
        [JsonIgnore]
        double EndTime { get; set; }

        /// <summary>
        /// The duration of the HitObject.
        /// </summary>
        double Duration { get; }
    }
}
