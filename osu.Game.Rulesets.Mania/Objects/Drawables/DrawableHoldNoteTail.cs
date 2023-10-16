// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The tail of a <see cref="DrawableHoldNote"/>.
    /// </summary>
    public partial class DrawableHoldNoteTail : DrawableNote
    {
        protected override ManiaSkinComponents Component => ManiaSkinComponents.HoldNoteTail;

        protected internal DrawableHoldNote HoldNote => (DrawableHoldNote)ParentHitObject;

        public DrawableHoldNoteTail()
            : this(null)
        {
        }

        public DrawableHoldNoteTail(TailNote tailNote)
            : base(tailNote)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override void CheckForResult(bool userTriggered, double timeOffset) =>
            // Factor in the release lenience
            base.CheckForResult(userTriggered, timeOffset / TailNote.RELEASE_WINDOW_LENIENCE);

        protected override HitResult GetCappedResult(HitResult result)
        {
            // If the head wasn't hit or the hold note was broken, cap the max score to Meh.
            bool hasComboBreak = !HoldNote.Head.IsHit || HoldNote.Body.HasHoldBreak;

            if (result > HitResult.Meh && hasComboBreak)
                return HitResult.Meh;

            return result;
        }

        public override bool OnPressed(KeyBindingPressEvent<ManiaAction> e) => false; // Handled by the hold note

        public override void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
        }
    }
}
