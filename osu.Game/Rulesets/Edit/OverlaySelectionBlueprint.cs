// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public abstract class OverlaySelectionBlueprint : SelectionBlueprint
    {
        /// <summary>
        /// The <see cref="DrawableHitObject"/> which this <see cref="OverlaySelectionBlueprint"/> applies to.
        /// </summary>
        public readonly DrawableHitObject DrawableObject;

        protected override bool ShouldBeAlive => (DrawableObject.IsAlive && DrawableObject.IsPresent) || State == SelectionState.Selected;

        protected OverlaySelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject.HitObject)
        {
            DrawableObject = drawableObject;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.ReceivePositionalInputAt(screenSpacePos);

        public override Vector2 SelectionPoint => DrawableObject.ScreenSpaceDrawQuad.Centre;

        public override Quad SelectionQuad => DrawableObject.ScreenSpaceDrawQuad;
    }
}
