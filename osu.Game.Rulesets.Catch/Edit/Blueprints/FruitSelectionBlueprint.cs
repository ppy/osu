// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public partial class FruitSelectionBlueprint : CatchSelectionBlueprint<Fruit>
    {
        private readonly FruitOutline outline;

        public FruitSelectionBlueprint(Fruit hitObject)
            : base(hitObject)
        {
            InternalChild = outline = new FruitOutline();
        }

        protected override void Update()
        {
            base.Update();

            if (!IsSelected) return;

            outline.Position = CatchHitObjectUtils.GetStartPosition(HitObjectContainer, HitObject);
            outline.UpdateFrom(HitObject);
        }
    }
}
