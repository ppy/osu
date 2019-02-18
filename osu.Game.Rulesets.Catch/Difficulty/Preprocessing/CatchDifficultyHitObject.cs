// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Difficulty.Preprocessing
{
    public class CatchDifficultyHitObject : DifficultyHitObject
    {
        private const float normalized_hitobject_radius = 41.0f;
        private const float absolute_player_positioning_error = 16f;

        public new CatchHitObject BaseObject => (CatchHitObject)base.BaseObject;

        public float MovementDistance { get; private set; }
        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="CatchDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

        private readonly float scalingFactor;

        private readonly CatchHitObject lastObject;

        public CatchDifficultyHitObject(HitObject hitObject, HitObject lastObject, double timeRate, float halfCatcherWidth)
            : base(hitObject, lastObject, timeRate)
        {
            this.lastObject = (CatchHitObject)lastObject;

            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            scalingFactor = normalized_hitobject_radius / halfCatcherWidth;

            setDistances();

            // Every strain interval is hard capped at the equivalent of 600 BPM streaming speed as a safety measure
            StrainTime = Math.Max(25, DeltaTime);
        }

        private void setDistances()
        {
            float lastPosition = getNormalizedPosition(lastObject);
            float currentPosition = getNormalizedPosition(BaseObject);

            // After a hyperdash the player is assumed to be in the correct position
            if (lastObject.LazyMovementDistance != null && !lastObject.HyperDash)
                lastPosition += lastObject.LazyMovementDistance.Value;

            BaseObject.LazyMovementDistance =
                MathHelper.Clamp(
                    lastPosition,
                    currentPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                    currentPosition + (normalized_hitobject_radius - absolute_player_positioning_error)) // Obtain new lazy position, but be stricter by allowing for an error of a certain degree of the player.
                - currentPosition; // Subtract HitObject position to obtain offset

            MovementDistance = Math.Abs(currentPosition - lastPosition + BaseObject.LazyMovementDistance.Value);

            if (currentPosition < lastPosition)
                MovementDistance *= -1;
        }

        private float getNormalizedPosition(CatchHitObject hitObject) => hitObject.X * CatchPlayfield.BASE_WIDTH * scalingFactor;
    }
}
