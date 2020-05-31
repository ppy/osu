// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public class HitCirclePlacementBlueprint : PlacementBlueprint
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        private readonly HitCirclePiece circlePiece;

        public HitCirclePlacementBlueprint()
            : base(new HitCircle())
        {
            InternalChild = circlePiece = new HitCirclePiece();
        }

        protected override void Update()
        {
            base.Update();

            circlePiece.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdatePosition(SnapResult result)
        {
            base.UpdatePosition(result);
            HitObject.Position = ToLocalSpace(result.ScreenSpacePosition);
        }
    }
}
