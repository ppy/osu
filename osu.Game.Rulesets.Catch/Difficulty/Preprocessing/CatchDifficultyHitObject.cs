// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Difficulty.Preprocessing
{
    public class CatchDifficultyHitObject : DifficultyHitObject
    {
        public const float NORMALIZED_HALF_CATCHER_WIDTH = 41.0f;
        private const float absolute_player_positioning_error = 16.0f;

        public new PalpableCatchHitObject BaseObject => (PalpableCatchHitObject)base.BaseObject;

        public new PalpableCatchHitObject LastObject => (PalpableCatchHitObject)base.LastObject;

        /// <summary>
        /// Normalized position of <see cref="BaseObject"/>.
        /// </summary>
        public readonly float NormalizedPosition;

        /// <summary>
        /// Normalized position of <see cref="LastObject"/>.
        /// </summary>
        public readonly float LastNormalizedPosition;

        /// <summary>
        /// Normalized position of the player required to catch <see cref="BaseObject"/>, assuming the player moves as little as possible.
        /// </summary>
        public float PlayerPosition { get; private set; }

        /// <summary>
        /// Normalized position of the player after catching <see cref="LastObject"/>.
        /// </summary>
        public float LastPlayerPosition { get; private set; }

        /// <summary>
        /// Normalized distance between <see cref="LastPlayerPosition"/> and <see cref="PlayerPosition"/>.
        /// </summary>
        /// <remarks>
        /// The sign of the value indicates the direction of the movement: negative is left and positive is right.
        /// </remarks>
        public float DistanceMoved { get; private set; }

        /// <summary>
        /// Normalized distance the player has to move from <see cref="LastPlayerPosition"/> in order to catch <see cref="BaseObject"/> at its <see cref="NormalizedPosition"/>.
        /// </summary>
        /// <remarks>
        /// The sign of the value indicates the direction of the movement: negative is left and positive is right.
        /// </remarks>
        public float ExactDistanceMoved { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="CatchDifficultyHitObject"/>, with a minimum of 40ms.
        /// </summary>
        public readonly double StrainTime;

        public CatchDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, float halfCatcherWidth, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = NORMALIZED_HALF_CATCHER_WIDTH / halfCatcherWidth;

            NormalizedPosition = BaseObject.EffectiveX * scalingFactor;
            LastNormalizedPosition = LastObject.EffectiveX * scalingFactor;

            // Every strain interval is hard capped at the equivalent of 375 BPM streaming speed as a safety measure
            StrainTime = Math.Max(40, DeltaTime);

            setMovementState();
        }

        private void setMovementState()
        {
            LastPlayerPosition = Index == 0 ? LastNormalizedPosition : ((CatchDifficultyHitObject)Previous(0)).PlayerPosition;

            PlayerPosition = Math.Clamp(
                LastPlayerPosition,
                NormalizedPosition - (NORMALIZED_HALF_CATCHER_WIDTH - absolute_player_positioning_error),
                NormalizedPosition + (NORMALIZED_HALF_CATCHER_WIDTH - absolute_player_positioning_error)
            );

            DistanceMoved = PlayerPosition - LastPlayerPosition;

            // For the exact position we consider that the catcher is in the correct position for both objects
            ExactDistanceMoved = NormalizedPosition - LastPlayerPosition;

            // After a hyperdash we ARE in the correct position. Always!
            if (LastObject.HyperDash)
                PlayerPosition = NormalizedPosition;
        }
    }
}
