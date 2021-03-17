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
    public class HitPlacementBlueprint : PlacementBlueprint
    {
        private readonly HitPiece piece;

        private static Hit hit;

        public HitPlacementBlueprint()
            : base(hit = new Hit())
        {
            InternalChild = piece = new HitPiece
            {
                Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.DEFAULT_HEIGHT)
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    hit.Type = HitType.Centre;
                    EndPlacement(true);
                    return true;

                case MouseButton.Right:
                    hit.Type = HitType.Rim;
                    EndPlacement(true);
                    return true;
            }

            return false;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            base.UpdateTimeAndPosition(result);
        }
    }
}
