// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners
{
    public partial class SpinnerPlacementBlueprint : PlacementBlueprint
    {
        public new Spinner HitObject => (Spinner)base.HitObject;

        private readonly SpinnerPiece piece;

        private bool isPlacingEnd;

        [Resolved]
        private IBeatSnapProvider? beatSnapProvider { get; set; }

        public SpinnerPlacementBlueprint()
            : base(new Spinner { Position = OsuPlayfield.BASE_SIZE / 2 })
        {
            InternalChild = piece = new SpinnerPiece { Alpha = 0.5f };
        }

        protected override void Update()
        {
            base.Update();

            if (isPlacingEnd)
                updateEndTimeFromCurrent();

            piece.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (isPlacingEnd)
            {
                if (e.Button != MouseButton.Right)
                    return false;

                updateEndTimeFromCurrent();
                EndPlacement(true);
            }
            else
            {
                if (e.Button != MouseButton.Left)
                    return false;

                BeginPlacement(commitStart: true);
                piece.FadeTo(1f, 150, Easing.OutQuint);

                isPlacingEnd = true;
            }

            return true;
        }

        private void updateEndTimeFromCurrent()
        {
            HitObject.EndTime = beatSnapProvider == null
                ? Math.Max(HitObject.StartTime, EditorClock.CurrentTime)
                : Math.Max(HitObject.StartTime + beatSnapProvider.GetBeatLengthAtTime(HitObject.StartTime), beatSnapProvider.SnapTime(EditorClock.CurrentTime));
        }
    }
}
