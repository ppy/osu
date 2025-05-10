// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public partial class DrawableHoldNoteBody : DrawableManiaHitObject<HoldNoteBody>
    {
        public bool HasHoldBreak => AllJudged && !IsHit;

        public override bool DisplayResult => false;

        public DrawableHoldNoteBody()
            : this(null)
        {
        }

        public DrawableHoldNoteBody(HoldNoteBody hitObject)
            : base(hitObject)
        {
        }

        internal void TriggerResult(bool hit)
        {
            if (AllJudged) return;

            if (hit)
                ApplyMaxResult();
            else
                ApplyMinResult();
        }
    }
}
