// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNotePlacementBlueprint : ManiaPlacementBlueprint<HoldNote>
    {
        private readonly EditNotePiece headPiece;
        private readonly EditNotePiece tailPiece;

        private PlacementState state;

        public HoldNotePlacementBlueprint()
            : base(new HoldNote())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                headPiece = new EditNotePiece { Origin = Anchor.Centre },
                tailPiece = new EditNotePiece { Origin = Anchor.Centre }
            };
        }

        protected override void Update()
        {
            base.Update();

            switch (state)
            {
                case PlacementState.Start:
                    headPiece.Position = SnappedMousePosition;
                    headPiece.Width = SnappedWidth;
                    break;
                case PlacementState.End:
                    tailPiece.Position = SnappedMousePosition;
                    tailPiece.Width = headPiece.Width;
                    break;
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Column column;
            if ((column = ColumnAt(e.ScreenSpaceMousePosition)) == null)
                return base.OnMouseDown(e);

            HitObject.StartTime = TimeAt(e.ScreenSpaceMousePosition);
            HitObject.Column = column.Index;

            BeginPlacement();

            state = PlacementState.End;

            return true;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            var endTime = TimeAt(e.ScreenSpaceMousePosition);
            if (endTime < HitObject.StartTime)
            {
                var tmp = endTime;
                endTime = HitObject.StartTime;
                HitObject.StartTime = tmp;
            }

            HitObject.Duration = endTime - HitObject.StartTime;

            EndPlacement();

            return true;
        }

        private enum PlacementState
        {
            Start,
            End
        }
    }
}
