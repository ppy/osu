// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

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

            piece.UpdateFrom(HitObject);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (isPlacingEnd)
            {
                HitObject.EndTime = EditorClock.CurrentTime;
                EndPlacement();
            }
            else
            {
                isPlacingEnd = true;
                piece.FadeTo(1f, 150, Easing.OutQuint);

                BeginPlacement();
            }

            return true;
        }

        public override void UpdatePosition(Vector2 screenSpacePosition)
        {
        }
    }
}
