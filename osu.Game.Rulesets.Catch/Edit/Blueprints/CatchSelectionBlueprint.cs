// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public abstract class CatchSelectionBlueprint<THitObject> : HitObjectSelectionBlueprint<THitObject>
        where THitObject : CatchHitObject
    {
        protected override bool AlwaysShowWhenSelected => true;

        public override Vector2 ScreenSpaceSelectionPoint
        {
            get
            {
                float x = HitObject.OriginalX;
                float y = HitObjectContainer.PositionAtTime(HitObject.StartTime);
                return HitObjectContainer.ToScreenSpace(new Vector2(x, y + HitObjectContainer.DrawHeight));
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => SelectionQuad.Contains(screenSpacePos);

        protected ScrollingHitObjectContainer HitObjectContainer => (ScrollingHitObjectContainer)playfield.HitObjectContainer;

        [Resolved]
        private Playfield playfield { get; set; }

        protected CatchSelectionBlueprint(THitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
