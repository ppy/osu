// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly bool HasTypeChange;
        public readonly bool HasTimingChange;
        public readonly TaikoDifficultyHitObjectRhythm Rhythm;
        public readonly bool IsKat;

        public bool StaminaCheese = false;

        public readonly int RhythmID;

        public readonly double NoteLength;

        public readonly int n;
        private int counter = 0;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {
            var lastHit = lastObject as Hit;
            var currentHit = hitObject as Hit;

            NoteLength = DeltaTime;
            double prevLength = (lastObject.StartTime - lastLastObject.StartTime) / clockRate;
            Rhythm = TaikoDifficultyHitObjectRhythm.GetClosest(NoteLength / prevLength);
            RhythmID = Rhythm.ID;
            HasTypeChange = lastHit?.Type != currentHit?.Type;
            IsKat = lastHit?.Type == HitType.Rim;
            HasTimingChange = !TaikoDifficultyHitObjectRhythm.IsRepeat(RhythmID);

            n = counter;
            counter++;
        }

        public const int CONST_RHYTHM_ID = 0;
    }
}
