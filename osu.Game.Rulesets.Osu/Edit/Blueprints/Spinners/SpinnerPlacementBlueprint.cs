// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners
{
    public class SpinnerPlacementBlueprint : PlacementBlueprint
    {
        public new Spinner HitObject => (Spinner)base.HitObject;

        private readonly SpinnerPiece piece;

        private bool isPlacingEnd;

        public SpinnerPlacementBlueprint()
            : base(new Spinner { Position = OsuPlayfield.BASE_SIZE / 2 })
        {
            InternalChild = piece = new SpinnerPiece { Alpha = 0.5f };
        }

        protected override void Update()
        {
            base.Update();

            if (isPlacingEnd)
                HitObject.EndTime = Math.Max(HitObject.StartTime, EditorClock.CurrentTime);

            piece.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (isPlacingEnd)
            {
                if (e.Button != MouseButton.Right)
                    return false;

                HitObject.EndTime = EditorClock.CurrentTime;
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

        public override void UpdatePosition(Vector2 screenSpacePosition)
        {
        }
    }
}
