// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public abstract class ManiaSkill : Skill
    {
        protected double CurrentChordTime { get; private set; }
        protected double PreviousChordTime { get; private set; }
        protected int ChordNoteCount { get; private set; }

        protected readonly double[] PreviousColumnTimes;

        // Used to update previousColumnTimes in a way unaffected by processing order.
        private readonly double[] currentColumnTimes;

        // Tracks if the chord starting at CurrentChordTime has already had PreprocessChordNote called for all notes
        private bool chordPreprocessed;

        public enum LnMode
        {
            Heads,
            Tails,
            Both
        }

        private readonly LnMode lnProcessingMode;

        protected ManiaSkill(Mod[] mods, int columns, LnMode lnProcessingMode = LnMode.Heads)
            : base(mods)
        {
            currentColumnTimes = new double[columns];
            PreviousColumnTimes = new double[columns];
            this.lnProcessingMode = lnProcessingMode;
        }

        public override void Process(DifficultyHitObject current)
        {
            if (!shouldProcess(current))
                return;

            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;

            // If this is a new time stamp, reset state for the next chord.
            if (maniaCurrent.StartTime > CurrentChordTime)
            {
                startNewChord(maniaCurrent.StartTime);
                chordPreprocessed = false;
            }

            if (!chordPreprocessed)
            {
                ManiaDifficultyHitObject? chordNote = maniaCurrent;

                // Look ahead and process all notes sharing this StartTime.
                while (chordNote != null && chordNote.StartTime == CurrentChordTime)
                {
                    PreprocessChordNote(chordNote);
                    currentColumnTimes[chordNote.Column] = chordNote.ActualTime;
                    ChordNoteCount++;
                    chordNote = getNext(chordNote);
                }

                chordPreprocessed = true;
            }

            FinalizeChord();

            double strainValue = ProcessInternal(maniaCurrent);

            ObjectDifficulties.Add(strainValue);
        }

        protected abstract void PreprocessChordNote(ManiaDifficultyHitObject current);
        protected abstract void FinalizeChord();

        private void startNewChord(double newChordTime)
        {
            // Reset skill specific chord implementations
            ResetChord();

            PreviousChordTime = CurrentChordTime;
            CurrentChordTime = newChordTime;
            ChordNoteCount = 0;

            for (int column = 0; column < currentColumnTimes.Length; column++)
                PreviousColumnTimes[column] = currentColumnTimes[column];
        }

        protected abstract void ResetChord();

        private bool shouldProcess(DifficultyHitObject current)
        {
            return current.BaseObject switch
            {
                TailNote => lnProcessingMode is LnMode.Tails or LnMode.Both,
                _ => lnProcessingMode is not LnMode.Tails
            };
        }

        private ManiaDifficultyHitObject? getNext(ManiaDifficultyHitObject curr)
        {
            return lnProcessingMode switch
            {
                LnMode.Heads => curr.NextHead(0),
                LnMode.Tails => curr.NextTail(0),
                LnMode.Both => (ManiaDifficultyHitObject?)curr.Next(0),
                _ => (ManiaDifficultyHitObject?)curr.Next(0)
            };
        }
    }
}
