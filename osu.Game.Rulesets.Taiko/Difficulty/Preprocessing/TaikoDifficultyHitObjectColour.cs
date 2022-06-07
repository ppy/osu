using System;

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
        /// If no repetition is found this will have a value of <see cref="max_repetition_interval"/> + 1.
        /// </summary>
        public int RepetitionInterval { get; private set; }

        /// <summary>
        /// Get the <see cref="TaikoDifficultyHitObjectColour"/> instance for the given hitObject. This is implemented
        /// as a static function instead of constructor to allow for reusing existing instances.
        /// TODO: findRepetitionInterval needs to be called a final time after all hitObjects have been processed.
        /// </summary>
        public static TaikoDifficultyHitObjectColour GetInstanceFor(TaikoDifficultyHitObject hitObject)
        {
            TaikoDifficultyHitObject lastObject = hitObject.PreviousNote(0);
            TaikoDifficultyHitObjectColour previous = lastObject?.Colour;
            bool delta = lastObject == null || hitObject.HitType != lastObject.HitType;
            if (previous != null && delta == previous.Delta)
            {
                previous.DeltaRunLength += 1;
                return previous;
            }
            else
            {
                // Calculate RepetitionInterval for previous
                previous?.FindRepetitionInterval();

                return new TaikoDifficultyHitObjectColour()
                {
                    Delta = delta,
                    DeltaRunLength = 1,
                    RepetitionInterval = max_repetition_interval + 1,
                    previous = previous
                };
            }
        }

        /// <summary>
        /// Finds the closest previous <see cref="TaikoDifficultyHitObjectColour"/> that has the identical delta value 
        /// and run length with the current instance, and returns the amount of notes between them.
        /// </summary>
        public void FindRepetitionInterval()
        {
            if (this.previous == null || this.previous.previous == null)
            {
                this.RepetitionInterval = max_repetition_interval + 1;
                return;
            }


            int interval = this.previous.DeltaRunLength;
            TaikoDifficultyHitObjectColour other = this.previous.previous;
            while (other != null && interval < max_repetition_interval)
            {
                interval += other.DeltaRunLength;
                if (other.Delta == this.Delta && other.DeltaRunLength == this.DeltaRunLength)
                {
                    this.RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    return;
                }

                other = other.previous;
            }

            this.RepetitionInterval = max_repetition_interval + 1;
        }
    }
}