// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public class HitCirclePlacementBlueprint : PlacementBlueprint
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        public HitCirclePlacementBlueprint()
            : base(new HitCircle())
        {
            InternalChild = new HitCirclePiece(HitObject);
        }

        protected override bool OnClick(ClickEvent e)
        {
            HitObject.StartTime = EditorClock.CurrentTime;
            EndPlacement();
            return true;
        }

        public override void UpdatePosition(Vector2 screenSpacePosition)
        {
            HitObject.Position = ToLocalSpace(screenSpacePosition);
        }
    }
}
