// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneOsuEditorGrids : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestGridExclusivity()
        {
            AddStep("enable distance snap grid", () => InputManager.Key(Key.T));
            AddStep("select second object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.ElementAt(1)));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());

            AddStep("enable rectangular grid", () => InputManager.Key(Key.Y));
            AddUntilStep("distance snap grid hidden", () => !this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddUntilStep("rectangular grid visible", () => this.ChildrenOfType<OsuRectangularPositionSnapGrid>().Any());

            AddStep("enable distance snap grid", () => InputManager.Key(Key.T));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddUntilStep("rectangular grid hidden", () => !this.ChildrenOfType<OsuRectangularPositionSnapGrid>().Any());
        }

        [Test]
        public void TestGridSizeToggling()
        {
            AddStep("enable rectangular grid", () => InputManager.Key(Key.Y));
            AddUntilStep("rectangular grid visible", () => this.ChildrenOfType<OsuRectangularPositionSnapGrid>().Any());
            gridSizeIs(4);

            nextGridSizeIs(8);
            nextGridSizeIs(16);
            nextGridSizeIs(32);
            nextGridSizeIs(4);
        }

        private void nextGridSizeIs(int size)
        {
            AddStep("toggle to next grid size", () => InputManager.Key(Key.G));
            gridSizeIs(size);
        }

        private void gridSizeIs(int size)
            => AddAssert($"grid size is {size}", () => this.ChildrenOfType<OsuRectangularPositionSnapGrid>().Single().Spacing == new Vector2(size));
    }
}
