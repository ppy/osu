// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public class HitCircleSelectionBlueprint : OsuSelectionBlueprint<HitCircle>
    {
        protected new DrawableHitCircle DrawableObject => (DrawableHitCircle)base.DrawableObject;

        protected readonly HitCirclePiece CirclePiece;

        public HitCircleSelectionBlueprint(DrawableHitCircle drawableCircle)
            : base(drawableCircle)
        {
            InternalChild = CirclePiece = new HitCirclePiece();
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(HitObject);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.HitArea.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => DrawableObject.HitArea.ScreenSpaceDrawQuad;
    }
}
