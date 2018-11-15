// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners
{
    public class SpinnerSelectionBlueprint : OsuSelectionBlueprint
    {
        private readonly SpinnerPiece piece;

        public SpinnerSelectionBlueprint(DrawableSpinner spinner)
            : base(spinner)
        {
            InternalChild = piece = new SpinnerPiece((Spinner)spinner.HitObject);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => piece.ReceivePositionalInputAt(screenSpacePos);

        public override void AdjustPosition(DragEvent dragEvent)
        {
            // Spinners don't support position adjustments
        }
    }
}
