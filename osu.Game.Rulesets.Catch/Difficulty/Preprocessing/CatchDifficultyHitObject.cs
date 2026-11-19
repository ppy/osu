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
        /// Normalized distance between <see cref="LastNormalizedPosition"/> and <see cref="NormalizedPosition"/>.
        /// </summary>
        public float Distance { get; private set; }

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="CatchDifficultyHitObject"/>, with a minimum of 40ms.
        /// </summary>
        public readonly double StrainTime;

        /// <summary>
        /// The direction of player movement between previous and current <see cref="CatchDifficultyHitObject"/>.
        /// </summary>
        public Direction MovementDirection;

        /// <summary>
        /// Subsequence of consecutive objects facing the same <see cref="MovementDirection"/> that the current <see cref="CatchDifficultyHitObject"/> is part of.
        /// If <see cref="MovementDirection"/> is equal to 0, it is added to the previous flow instead of making a new flow.
        /// The last <see cref="CatchDifficultyHitObject"/> of each flow is a direction change.
        /// </summary>
        public Flow Flow;

        public CatchDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, float halfCatcherWidth, List<DifficultyHitObject> objects, int index, List<Flow> flows)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = NORMALIZED_HALF_CATCHER_WIDTH / halfCatcherWidth;

            NormalizedPosition = BaseObject.EffectiveX * scalingFactor;
            LastNormalizedPosition = LastObject.EffectiveX * scalingFactor;

            // Every strain interval is hard capped at the equivalent of 375 BPM streaming speed as a safety measure
            StrainTime = Math.Max(40, DeltaTime);

            setMovementState();

            // Divide all hit objects into flows, i.e. series of consecutive notes facing the same direction.
            var prev = (CatchDifficultyHitObject)Previous(0);

            if (prev != null && (prev.Flow.MovementDirection == MovementDirection || MovementDirection == Direction.None))
                {
                    prev.Flow.Add(this);
                    Flow = prev.Flow;
                }
            else // Create a new flow on direction change (or the first object of the map).
            {
                Flow = new Flow(new List<CatchDifficultyHitObject> { this }, flows, flows.Count);
                flows.Add(Flow);
            }
        }

        private void setMovementState()
        {
            var previousObj = (CatchDifficultyHitObject?)Previous();

            LastPlayerPosition = previousObj?.PlayerPosition ?? LastNormalizedPosition;

            PlayerPosition = Math.Clamp(
                LastPlayerPosition,
                NormalizedPosition - (NORMALIZED_HALF_CATCHER_WIDTH - absolute_player_positioning_error),
                NormalizedPosition + (NORMALIZED_HALF_CATCHER_WIDTH - absolute_player_positioning_error)
            );

            DistanceMoved = PlayerPosition - LastPlayerPosition;
            Distance = NormalizedPosition - LastNormalizedPosition;

            // After a hyperdash we ARE in the correct position. Always!
            if (LastObject.HyperDash)
                PlayerPosition = NormalizedPosition;

            MovementDirection = (Direction)Math.Sign(DistanceMoved);
        }
    }
}
