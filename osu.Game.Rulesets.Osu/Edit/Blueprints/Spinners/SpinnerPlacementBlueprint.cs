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
    public partial class SpinnerPlacementBlueprint : HitObjectPlacementBlueprint
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
                // even if the mouse button isn't the correct button to terminate placement,
                // clicks must not be allowed to fall through to underlying drawables.
                // if they were allowed to fall through, they may trigger selection of some other object.
                // selection of some other object will cause the tool to revert to "select",
                // which will cause this placement to automatically commit,
                // which *may* also cause the removal of the object whose selection was attempted
                // (happens when the start time of this spinner is the same as the start time of clicked object),
                // and that last possibility *will* result in a hard crash.
                if (e.Button != MouseButton.Right)
                    return true;

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
