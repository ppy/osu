// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Spinners
{
    public partial class SpinnerSelectionBlueprint : OsuSelectionBlueprint<Spinner>
    {
        private readonly SpinnerPiece piece;

        public SpinnerSelectionBlueprint(Spinner spinner)
            : base(spinner)
        {
            InternalChild = piece = new SpinnerPiece();
        }

        protected override void Update()
        {
            base.Update();

            piece.UpdateFrom(HitObject);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => piece.ReceivePositionalInputAt(screenSpacePos);
    }
}
