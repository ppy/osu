// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class NotePlacementBlueprint : ManiaPlacementBlueprint<Note>
    {
        private Circle piece = null!;

        public NotePlacementBlueprint()
            : base(new Note())
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChild = piece = new Circle
            {
                Origin = Anchor.Centre,
                Colour = colours.Yellow,
                Height = 10
            };
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double referenceTime)
        {
            var result = base.UpdateTimeAndPosition(screenSpacePosition, referenceTime);

            if (result.Playfield != null)
            {
                piece.Width = result.Playfield.DrawWidth;
                piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            }

            return result;
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
