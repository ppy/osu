// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public class CatchPlacementBlueprint<THitObject> : PlacementBlueprint
        where THitObject : CatchHitObject, new()
    {
        protected new THitObject HitObject => (THitObject)base.HitObject;

        protected ScrollingHitObjectContainer HitObjectContainer => (ScrollingHitObjectContainer)playfield.HitObjectContainer;

        [Resolved]
        private Playfield playfield { get; set; }

        public CatchPlacementBlueprint()
            : base(new THitObject())
        {
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
    }
}
