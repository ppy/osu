namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        const int max_repetition_interval = 16;

        private TaikoDifficultyHitObjectColour previous;

        /// <summary>
        /// True if the current colour is different from the previous colour.
        /// </summary>
        public bool Delta { get; private set; }

        /// <summary>
        /// How many notes are Delta repeated
        /// </summary>
        public int DeltaRunLength { get; private set; }

        /// <summary>
        /// How many notes between the current and previous identical <see cref="TaikoDifficultyHitObjectColour"/>.
        /// Negative number means that there is no repetition in range.
        /// </summary>
        public int RepetitionInterval { get; private set; }

        /// <summary>
        /// Get the <see cref="TaikoDifficultyHitObjectColour"/> instance for the given hitObject. This is implemented
        /// as a static function instead of constructor to allow for reusing existing instances.
        /// TODO: findRepetitionInterval needs to be called a final time after all hitObjects have been processed.
        /// </summary>
        public static TaikoDifficultyHitObjectColour GetInstanceFor(
            TaikoDifficultyHitObject hitObject, TaikoDifficultyHitObject lastObject, TaikoDifficultyHitObjectColour previous)
        {
            bool delta = lastObject == null || hitObject.HitType != lastObject.HitType;
            if (delta == previous.Delta)
            {
                previous.DeltaRunLength += 1;
                return previous;
            }
            else
            {
                // Calculate RepetitionInterval for previous
                previous.RepetitionInterval = findRepetitionInterval(previous);

                return new TaikoDifficultyHitObjectColour()
                {
                    Delta = delta,
                    DeltaRunLength = 1,
                    RepetitionInterval = -1,
                    previous = previous
                };
            }
        }

        /// <summary>
        /// Finds the closest previous <see cref="TaikoDifficultyHitObjectColour"/> that has the identical delta value 
        /// and run length to target, and returns the amount of notes between them.
        /// </summary>
        private static int findRepetitionInterval(TaikoDifficultyHitObjectColour target) {
            if (target.previous == null || target.previous.previous == null)
                return -1;

            int interval = target.previous.DeltaRunLength;
            TaikoDifficultyHitObjectColour other = target.previous.previous;
            while(other != null && interval < max_repetition_interval) {
                if (other.Delta == target.Delta && other.DeltaRunLength == target.DeltaRunLength)
                    return interval;
                else
                    interval += other.DeltaRunLength;
                other = other.previous;
            }

            return -1;
        }
    }
}