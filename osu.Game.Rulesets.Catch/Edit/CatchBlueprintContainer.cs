// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Edit.Blueprints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchBlueprintContainer : ComposeBlueprintContainer
    {
        public CatchBlueprintContainer(CatchHitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new CatchSelectionHandler();

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Fruit fruit:
                    return new FruitSelectionBlueprint(fruit);

                case JuiceStream juiceStream:
                    return new JuiceStreamSelectionBlueprint(juiceStream);

                case BananaShower bananaShower:
                    return new BananaShowerSelectionBlueprint(bananaShower);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        protected sealed override DragBox CreateDragBox() => new ScrollingDragBox(Composer.Playfield);
    }
}
