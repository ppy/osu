// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Scoring
{
    public readonly struct HitEvent
    {
        /// <summary>
        /// The time offset from the end of <see cref="HitObject"/> at which the event occurred.
        /// </summary>
        public readonly double TimeOffset;

        /// <summary>
        /// The hit result.
        /// </summary>
        public readonly HitResult Result;

        /// <summary>
        /// The <see cref="HitObject"/> on which the result occurred.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The <see cref="HitObject"/> occurring prior to <see cref="HitObject"/>.
        /// </summary>
        [CanBeNull]
        public readonly HitObject LastHitObject;

        /// <summary>
        /// The player's position offset, if available, at the time of the event.
        /// </summary>
        [CanBeNull]
        public readonly Vector2? PositionOffset;

        public HitEvent(double timeOffset, HitResult result, HitObject hitObject, [CanBeNull] HitObject lastHitObject, [CanBeNull] Vector2? positionOffset)
        {
            TimeOffset = timeOffset;
            Result = result;
            HitObject = hitObject;
            LastHitObject = lastHitObject;
            PositionOffset = positionOffset;
        }

        public HitEvent With(Vector2? positionOffset) => new HitEvent(TimeOffset, Result, HitObject, LastHitObject, positionOffset);
    }
}
