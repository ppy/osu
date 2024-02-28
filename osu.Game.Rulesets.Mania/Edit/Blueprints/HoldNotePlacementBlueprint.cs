// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class HoldNotePlacementBlueprint : ManiaPlacementBlueprint<HoldNote>
    {
        private readonly EditBodyPiece bodyPiece;
        private readonly EditNotePiece headPiece;
        private readonly EditNotePiece tailPiece;

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

        protected override bool IsValidForPlacement => Precision.DefinitelyBigger(HitObject.Duration, 0);

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
                headPiece.Y = Parent!.ToLocalSpace(Column.ScreenSpacePositionAtTime(HitObject.StartTime)).Y;
                tailPiece.Y = Parent!.ToLocalSpace(Column.ScreenSpacePositionAtTime(HitObject.EndTime)).Y;

                switch (scrollingInfo.Direction.Value)
                {
                    case ScrollingDirection.Down:
                        headPiece.Y -= headPiece.DrawHeight / 2;
                        tailPiece.Y -= tailPiece.DrawHeight / 2;
                        break;

                    case ScrollingDirection.Up:
                        headPiece.Y += headPiece.DrawHeight / 2;
                        tailPiece.Y += tailPiece.DrawHeight / 2;
                        break;
                }
            }

            var topPosition = new Vector2(headPiece.DrawPosition.X, Math.Min(headPiece.DrawPosition.Y, tailPiece.DrawPosition.Y));
            var bottomPosition = new Vector2(headPiece.DrawPosition.X, Math.Max(headPiece.DrawPosition.Y, tailPiece.DrawPosition.Y));

            bodyPiece.Position = topPosition;
            bodyPiece.Width = headPiece.Width;
            bodyPiece.Height = (bottomPosition - topPosition).Y;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
                return;

            base.OnMouseUp(e);
            EndPlacement(true);
        }

        private double originalStartTime;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double endTime)
                {
                    HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
                    HitObject.Duration = Math.Abs(endTime - originalStartTime);
                }
            }
            else
            {
                if (result.Playfield != null)
                {
                    headPiece.Width = tailPiece.Width = result.Playfield.DrawWidth;
                    headPiece.X = tailPiece.X = ToLocalSpace(result.ScreenSpacePosition).X;
                }

                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
        }
    }
}
