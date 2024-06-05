// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class HitPlacementBlueprint : PlacementBlueprint
    {
        private readonly HitPiece piece;

        public new Hit HitObject => (Hit)base.HitObject;

        public HitPlacementBlueprint()
            : base(new Hit())
        {
            InternalChild = piece = new HitPiece
            {
                Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.BASE_HEIGHT)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeginPlacement();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            EndPlacement(true);
            return true;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            base.UpdateTimeAndPosition(result);
        }
    }
}
