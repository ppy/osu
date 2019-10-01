// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public class HitCircleSelectionBlueprint : OsuSelectionBlueprint<HitCircle>
    {
        protected readonly HitCirclePiece CirclePiece;

        public HitCircleSelectionBlueprint(DrawableHitCircle hitCircle)
            : base(hitCircle)
        {
            InternalChild = CirclePiece = new HitCirclePiece();
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(HitObject);
        }
    }
}
