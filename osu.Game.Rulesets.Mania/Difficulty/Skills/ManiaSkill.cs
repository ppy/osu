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
            ManiaDifficultyHitObject maniaCurrent = (ManiaDifficultyHitObject)current;

            PreChordProcess(maniaCurrent);

            // If this is a new time stamp, reset state for the next chord.
            if (maniaCurrent.StartTime > CurrentChordTime && ShouldProcess(maniaCurrent))
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
                    if (ShouldProcess(chordNote))
                    {
                        PreprocessChordNote(chordNote);
                        currentColumnTimes[chordNote.Column] = chordNote.ActualTime;
                        ChordNoteCount++;
                    }

                    chordNote = getNext(chordNote);
                }

                FinalizeChord();

                chordPreprocessed = true;
            }

            double strainValue = ShouldProcess(maniaCurrent) ? ProcessInternal(maniaCurrent) : ReadonlyStrainValueAt(maniaCurrent.ActualTime, maniaCurrent);

            ObjectDifficulties.Add(strainValue);
        }

        /// <summary>
        /// Processing steps to take before the chord begins its processing.
        /// </summary>
        /// <param name="current">The current note to process at.</param>
        protected virtual void PreChordProcess(ManiaDifficultyHitObject current) { }

        /// <summary>
        /// Processing steps to take on each note before the chord is finalized. Difficulty should generally be converted to strain here.
        /// </summary>
        /// <param name="current">The current note in this chord.</param>
        protected abstract void PreprocessChordNote(ManiaDifficultyHitObject current);

        /// <summary>
        /// Chord-specific cleanup actions to take when the chord is finished its preprocessing.
        /// </summary>
        protected abstract void FinalizeChord();

        /// <summary>
        /// The strain value at the current note in this chord. Should generally avoid touching data processed in <see cref="PreprocessChordNote"/>.
        /// </summary>
        /// <param name="current">The note to get the strain value at.</param>
        /// <returns>The strain value of the note.</returns>
        protected abstract double StrainValueAt(ManiaDifficultyHitObject current);

        /// <summary>
        /// Post-processing steps taken to reset any chord-specific data processed in <see cref="PreprocessChordNote"/>.
        /// </summary>
        protected abstract void ResetChord();

        /// <summary>
        /// A readonly strain value used to peek at current strain values, while avoiding any changes that may occur in <see cref="StrainValueAt"/>.
        /// </summary>
        /// <param name="time">The time to peek at strain at.</param>
        /// <param name="current">The previous note to this current strain value calculation.</param>
        /// <returns>The strain value at this point.</returns>
        protected abstract double ReadonlyStrainValueAt(double time, ManiaDifficultyHitObject current);

        private void startNewChord(double newChordTime)
        {
            // Reset skill specific chord implementations
            PreviousChordTime = CurrentChordTime;
            CurrentChordTime = newChordTime;
            ChordNoteCount = 0;

            for (int column = 0; column < currentColumnTimes.Length; column++)
                PreviousColumnTimes[column] = currentColumnTimes[column];

            ResetChord();
        }

        /// <summary>
        /// Determines if this note should be processed under the current <see cref="lnProcessingMode"/>.
        /// </summary>
        /// <param name="current">The note to check.</param>
        /// <returns>Whether this note should contribute to strain processing (if not, it shall be readonly).</returns>
        protected bool ShouldProcess(DifficultyHitObject current)
        {
            return current.BaseObject switch
            {
                TailNote => lnProcessingMode is LnMode.Tails or LnMode.Both,
                _ => lnProcessingMode is not LnMode.Tails
            };
        }

        /// <summary>
        /// Gets the next note that should contribute to processing under the current <see cref="lnProcessingMode"/>
        /// </summary>
        /// <param name="current">The current note.</param>
        /// <returns>The next note in sequence that contributes to processing.</returns>
        private ManiaDifficultyHitObject? getNext(ManiaDifficultyHitObject current)
        {
            return lnProcessingMode switch
            {
                LnMode.Heads => current.NextHead(0),
                LnMode.Tails => current.NextTail(0),
                _ => (ManiaDifficultyHitObject?)current.Next(0)
            };
        }
    }
}
