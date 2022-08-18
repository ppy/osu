// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneSliderSplitting : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private ContextMenuContainer contextMenuContainer
            => Editor.ChildrenOfType<ContextMenuContainer>().First();

        private Slider? slider;
        private PathControlPointVisualiser? visualiser;

        [Test]
        public void TestBasicSplit()
        {
            AddStep("add slider", () =>
            {
                slider = new Slider
                {
                    Position = new Vector2(0, 50),
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero, PathType.PerfectCurve),
                        new PathControlPoint(new Vector2(150, 150)),
                        new PathControlPoint(new Vector2(300, 0), PathType.PerfectCurve),
                        new PathControlPoint(new Vector2(400, 0)),
                        new PathControlPoint(new Vector2(400, 150))
                    })
                };

                EditorBeatmap.Add(slider);
            });

            AddStep("select added slider", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(slider);
                visualiser = blueprintContainer.SelectionBlueprints.First(o => o.Item == slider).ChildrenOfType<PathControlPointVisualiser>().First();
            });

            moveMouseToControlPoint(2);
            AddStep("select control point", () =>
            {
                if (visualiser is not null) visualiser.Pieces[2].IsSelected.Value = true;
            });
            addContextMenuItemStep("Split control point");
        }

        [Test]
        public void TestStartTimeOffsetPlusDeselect()
        {
            HitCircle? circle = null;

            AddStep("add circle", () =>
            {
                circle = new HitCircle();

                EditorBeatmap.Add(circle);
            });

            AddStep("select added circle", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(circle);
            });

            AddStep("add another circle", () =>
            {
                var circle2 = new HitCircle();

                EditorBeatmap.Add(circle2);
            });

            AddStep("change time of selected circle and deselect", () =>
            {
                if (circle is null) return;

                circle.StartTime += 1;
                EditorBeatmap.SelectedHitObjects.Clear();
            });
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                if (slider is null || visualiser is null) return;

                Vector2 position = slider.Path.ControlPoints[index].Position + slider.Position;
                InputManager.MoveMouseTo(visualiser.Pieces[0].Parent.ToScreenSpace(position));
            });
        }

        private void addContextMenuItemStep(string contextMenuText)
        {
            AddStep($"click context menu item \"{contextMenuText}\"", () =>
            {
                if (visualiser is null) return;

                MenuItem? item = visualiser.ContextMenuItems.FirstOrDefault(menuItem => menuItem.Text.Value == contextMenuText);

                item?.Action?.Value();
            });
        }
    }
}
