// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Visual;
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
            AddUntilStep("rectangular grid visible", () => this.ChildrenOfType<RectangularPositionSnapGrid>().Any());

            AddStep("enable distance snap grid", () => InputManager.Key(Key.T));
            AddUntilStep("distance snap grid visible", () => this.ChildrenOfType<OsuDistanceSnapGrid>().Any());
            AddUntilStep("rectangular grid hidden", () => !this.ChildrenOfType<RectangularPositionSnapGrid>().Any());
        }
    }
}
