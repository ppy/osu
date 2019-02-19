// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    public class DifficultyHitObject
    {
        /// <summary>
        /// Milliseconds elapsed since the <see cref="HitObject.StartTime"/> of the previous <see cref="DifficultyHitObject"/>.
        /// </summary>
        public double DeltaTime { get; private set; }

        /// <summary>
        /// The <see cref="HitObject"/> this <see cref="DifficultyHitObject"/> refers to.
        /// </summary>
        public readonly HitObject BaseObject;

        /// <summary>
        /// The previous <see cref="HitObject"/> to <see cref="BaseObject"/>.
        /// </summary>
        public readonly HitObject LastObject;

        public DifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate)
        {
            BaseObject = hitObject;
            LastObject = lastObject;
            DeltaTime = (hitObject.StartTime - lastObject.StartTime) / clockRate;
        }
    }
}
