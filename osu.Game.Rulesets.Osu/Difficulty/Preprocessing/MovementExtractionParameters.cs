// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    internal class MovementExtractionParameters
    {
        [CanBeNull]
        public OsuHitObject FourthLastObject { get; }

        [CanBeNull]
        public OsuHitObject SecondLastObject { get; }

        public OsuHitObject LastObject { get; }

        public OsuHitObject CurrentObject { get; }

        [CanBeNull]
        public OsuHitObject NextObject { get; }

        public OsuObjectPair? FourthLastToCurrent { get; }

        public OsuObjectPair? SecondLastToLast { get; }
        public OsuObjectPair? SecondLastToCurrent { get; }
        public OsuObjectPair? SecondLastToNext { get; }

        public OsuObjectPair LastToCurrent { get; }
        public OsuObjectPair? LastToNext { get; }

        public OsuObjectPair? CurrentToNext { get; }

        public double EffectiveBPM { get; }

        public bool LastObjectTemporallyCenteredBetweenNeighbours { get; set; }
        public bool CurrentObjectTemporallyCenteredBetweenNeighbours { get; set; }

        public double SecondLastToCurrentFlowiness { get; set; }
        public double LastToNextFlowiness { get; set; }

        public double Cheesability { get; set; }
        public double CheeseWindow { get; set; }

        public MovementExtractionParameters(
            [CanBeNull] OsuHitObject fourthLastObject,
            [CanBeNull] OsuHitObject secondLastObject,
            OsuHitObject lastObject,
            OsuHitObject currentObject,
            [CanBeNull] OsuHitObject nextObject,
            double gameplayRate)
        {
            FourthLastObject = fourthLastObject;
            SecondLastObject = secondLastObject is Spinner ? null : secondLastObject;
            LastObject = lastObject;
            CurrentObject = currentObject;
            NextObject = nextObject is Spinner ? null : nextObject;

            FourthLastToCurrent = OsuObjectPair.Nullable(FourthLastObject, CurrentObject, gameplayRate);

            SecondLastToLast = OsuObjectPair.Nullable(SecondLastObject, LastObject, gameplayRate);
            SecondLastToCurrent = OsuObjectPair.Nullable(SecondLastObject, CurrentObject, gameplayRate);
            SecondLastToNext = OsuObjectPair.Nullable(SecondLastObject, NextObject, gameplayRate);

            LastToCurrent = new OsuObjectPair(LastObject, CurrentObject, gameplayRate);
            LastToNext = OsuObjectPair.Nullable(LastObject, NextObject, gameplayRate);

            CurrentToNext = OsuObjectPair.Nullable(CurrentObject, NextObject, gameplayRate);

            EffectiveBPM = 30 / (LastToCurrent.TimeDelta + 1e-10);

            LastObjectTemporallyCenteredBetweenNeighbours = false;
            CurrentObjectTemporallyCenteredBetweenNeighbours = false;

            SecondLastToCurrentFlowiness = 0;
            LastToNextFlowiness = 0;

            Cheesability = 0;
            CheeseWindow = 0;
        }
    }
}
