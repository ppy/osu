// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNotePlacementBlueprint : ManiaPlacementBlueprint<HoldNote>
    {
        private readonly EditBodyPiece bodyPiece;
        private readonly EditNotePiece headPiece;
        private readonly EditNotePiece tailPiece;

        public HoldNotePlacementBlueprint()
            : base(new HoldNote())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                bodyPiece = new EditBodyPiece { Origin = Anchor.TopCentre },
                headPiece = new EditNotePiece { Origin = Anchor.Centre },
                tailPiece = new EditNotePiece { Origin = Anchor.Centre }
            };
        }

        protected override void Update()
        {
            base.Update();

            if (Column != null)
            {
                headPiece.Y = PositionAt(HitObject.StartTime);
                tailPiece.Y = PositionAt(HitObject.EndTime);
            }

            var topPosition = new Vector2(headPiece.DrawPosition.X, Math.Min(headPiece.DrawPosition.Y, tailPiece.DrawPosition.Y));
            var bottomPosition = new Vector2(headPiece.DrawPosition.X, Math.Max(headPiece.DrawPosition.Y, tailPiece.DrawPosition.Y));

            bodyPiece.Position = topPosition;
            bodyPiece.Width = headPiece.Width;
            bodyPiece.Height = (bottomPosition - topPosition).Y;
        }

        private double originalStartTime;

        public override void UpdatePosition(Vector2 screenSpacePosition)
        {
            base.UpdatePosition(screenSpacePosition);

            if (PlacementActive)
            {
                var endTime = TimeAt(screenSpacePosition);

                HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                HitObject.Duration = Math.Abs(endTime - originalStartTime);
            }
            else
            {
                headPiece.Width = tailPiece.Width = SnappedWidth;
                headPiece.X = tailPiece.X = SnappedMousePosition.X;

                originalStartTime = HitObject.StartTime = TimeAt(screenSpacePosition);
            }
        }
    }
}
