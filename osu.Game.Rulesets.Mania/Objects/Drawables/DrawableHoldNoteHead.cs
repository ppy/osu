// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The head of a <see cref="DrawableHoldNote"/>.
    /// </summary>
    public class DrawableHoldNoteHead : DrawableNote
    {
        protected override ManiaSkinComponents Component => ManiaSkinComponents.HoldNoteHead;

        public DrawableHoldNoteHead()
            : this(null)
        {
        }

        public DrawableHoldNoteHead(HeadNote headNote)
            : base(headNote)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            // This hitobject should never expire, so this is just a safe maximum.
            LifetimeEnd = LifetimeStart + 30000;
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            // suppress the base call explicitly.
            // the hold note head should never change its visual state on its own due to the "freezing" mechanic
            // (when hit, it remains visible in place at the judgement line; when dropped, it will scroll past the line).
            // it will be hidden along with its parenting hold note when required.
        }

        public override bool OnPressed(ManiaAction action) => false; // Handled by the hold note

        public override void OnReleased(ManiaAction action)
        {
        }
    }
}
