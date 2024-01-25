// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public partial class DrawableHoldNoteBody : DrawableManiaHitObject<HoldNoteBody>
    {
        public bool HasHoldBreak => AllJudged && !IsHit;

        public override bool DisplayResult => false;

        private bool hit;

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

            this.hit = hit;
            ApplyResult(static (r, hitObject) =>
            {
                var holdNoteBody = (DrawableHoldNoteBody)hitObject;
                r.Type = holdNoteBody.hit ? r.Judgement.MaxResult : r.Judgement.MinResult;
            });
        }
    }
}
