// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public abstract class CatchPlacementBlueprintTestScene : PlacementBlueprintTestScene
    {
        protected new ScrollingHitObjectContainer HitObjectContainer => contentContainer.Playfield.HitObjectContainer;

        protected override Container<Drawable> Content => contentContainer;

        private readonly CatchEditorTestSceneContainer contentContainer;

        protected CatchPlacementBlueprintTestScene()
        {
            base.Content.Add(contentContainer = new CatchEditorTestSceneContainer
            {
                Clock = new FramedClock(new ManualClock())
            });
        }

        // Unused because AddHitObject is overriden
        protected override Container CreateHitObjectContainer() => new Container();

        protected override void AddHitObject(DrawableHitObject hitObject)
        {
            contentContainer.Playfield.HitObjectContainer.Add(hitObject);
        }

        protected override SnapResult SnapForBlueprint(PlacementBlueprint blueprint)
        {
            var result = base.SnapForBlueprint(blueprint);
            result.Time = HitObjectContainer.TimeAtScreenSpacePosition(result.ScreenSpacePosition);
            return result;
        }
    }
}
