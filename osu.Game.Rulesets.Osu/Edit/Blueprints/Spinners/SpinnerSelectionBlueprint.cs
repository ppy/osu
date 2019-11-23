// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

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
    }
}
