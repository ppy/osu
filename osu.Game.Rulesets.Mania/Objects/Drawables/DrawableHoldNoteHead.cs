// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The head of a <see cref="DrawableHoldNote"/>.
    /// </summary>
    public class DrawableHoldNoteHead : DrawableNote
    {
        private readonly DrawableHoldNote holdNote;

        public DrawableHoldNoteHead(DrawableHoldNote holdNote)
            : base(holdNote.HitObject.Head)
        {
            this.holdNote = holdNote;
        }

        public override bool OnPressed(ManiaAction action)
        {
            if (!base.OnPressed(action))
                return false;

            // If the key has been released too early, the user should not receive full score for the release
            if (Result.Type == HitResult.Miss)
                holdNote.HasBroken = true;

            // The head note also handles early hits before the body, but we want accurate early hits to count as the body being held
            // The body doesn't handle these early early hits, so we have to explicitly set the holding state here
            holdNote.BeginHold();

            return true;
        }
    }
}
