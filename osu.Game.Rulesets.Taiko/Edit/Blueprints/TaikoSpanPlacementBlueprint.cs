// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class TaikoSpanPlacementBlueprint : PlacementBlueprint
    {
        private readonly HitPiece headPiece;
        private readonly HitPiece tailPiece;

        private readonly LengthPiece lengthPiece;

        private readonly IHasDuration spanPlacementObject;

        protected override bool IsValidForPlacement => Precision.DefinitelyBigger(spanPlacementObject.Duration, 0);

        public TaikoSpanPlacementBlueprint(HitObject hitObject)
            : base(hitObject)
        {
            spanPlacementObject = hitObject as IHasDuration;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                headPiece = new HitPiece
                {
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.BASE_HEIGHT)
                },
                lengthPiece = new LengthPiece
                {
                    Height = TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.BASE_HEIGHT
                },
                tailPiece = new HitPiece
                {
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.BASE_HEIGHT)
                }
            };
        }

        private double originalStartTime;
        private Vector2 originalPosition;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeginPlacement();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            BeginPlacement(true);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
                return;

            base.OnMouseUp(e);
            EndPlacement(true);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (PlacementActive == PlacementState.Active)
            {
                if (result.Time is double dragTime)
                {
                    if (dragTime < originalStartTime)
                    {
                        HitObject.StartTime = dragTime;
                        spanPlacementObject.Duration = Math.Abs(dragTime - originalStartTime);
                        headPiece.Position = ToLocalSpace(result.ScreenSpacePosition);
                        tailPiece.Position = originalPosition;
                    }
                    else
                    {
                        HitObject.StartTime = originalStartTime;
                        spanPlacementObject.Duration = Math.Abs(dragTime - originalStartTime);
                        tailPiece.Position = ToLocalSpace(result.ScreenSpacePosition);
                        headPiece.Position = originalPosition;
                    }

                    lengthPiece.X = headPiece.X;
                    lengthPiece.Width = tailPiece.X - headPiece.X;
                }
            }
            else
            {
                lengthPiece.Position = headPiece.Position = tailPiece.Position = ToLocalSpace(result.ScreenSpacePosition);

                if (result.Time is double startTime)
                {
                    originalStartTime = HitObject.StartTime = startTime;
                    originalPosition = ToLocalSpace(result.ScreenSpacePosition);
                }
            }
        }
    }
}
