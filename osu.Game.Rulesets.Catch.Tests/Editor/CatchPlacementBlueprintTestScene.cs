// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public abstract partial class CatchPlacementBlueprintTestScene : PlacementBlueprintTestScene
    {
        protected const double TIME_SNAP = 100;

        protected DrawableCatchHitObject LastObject;

        protected new ScrollingHitObjectContainer HitObjectContainer => contentContainer.Playfield.HitObjectContainer;

        protected override Container<Drawable> Content => contentContainer;

        private readonly CatchEditorTestSceneContainer contentContainer;

        protected CatchPlacementBlueprintTestScene()
        {
            base.Content.Add(contentContainer = new CatchEditorTestSceneContainer());

            contentContainer.Playfield.Clock = new FramedClock(new ManualClock());
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            HitObjectContainer.Clear();
            ResetPlacement();
            LastObject = null;
        });

        protected void AddMoveStep(double time, float x) => AddStep($"move to time={time}, x={x}", () =>
        {
            float y = HitObjectContainer.PositionAtTime(time);
            Vector2 pos = HitObjectContainer.ToScreenSpace(new Vector2(x, y + HitObjectContainer.DrawHeight));
            InputManager.MoveMouseTo(pos);
        });

        protected void AddClickStep(MouseButton button) => AddStep($"click {button}", () =>
        {
            InputManager.Click(button);
        });

        protected IEnumerable<FruitOutline> FruitOutlines => Content.ChildrenOfType<FruitOutline>();

        // Unused because AddHitObject is overriden
        protected override Container CreateHitObjectContainer() => new Container();

        protected override void AddHitObject(DrawableHitObject hitObject)
        {
            LastObject = (DrawableCatchHitObject)hitObject;
            contentContainer.Playfield.HitObjectContainer.Add(hitObject);
        }

        protected override SnapResult SnapForBlueprint(PlacementBlueprint blueprint)
        {
            var result = base.SnapForBlueprint(blueprint);
            result.Time = Math.Round(HitObjectContainer.TimeAtScreenSpacePosition(result.ScreenSpacePosition) / TIME_SNAP) * TIME_SNAP;
            return result;
        }
    }
}
