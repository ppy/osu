// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class NotePlacementBlueprint : ManiaPlacementBlueprint<Note>
    {
        private readonly EditNotePiece piece;

        public NotePlacementBlueprint()
            : base(new Note())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = piece = new EditNotePiece { Origin = Anchor.Centre };
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (result.Playfield != null)
            {
                piece.Width = result.Playfield.DrawWidth;
                piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            base.OnMouseDown(e);

            // Place the note immediately.
            EndPlacement(true);

            return true;
        }
    }
}
