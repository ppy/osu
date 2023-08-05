// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneOsuComposerSelection : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestSelectAfterFadedOut()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));

            moveMouseToObject(() => slider);

            AddStep("seek after end", () => EditorClock.Seek(750));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("slider not selected", () => EditorBeatmap.SelectedHitObjects.Count == 0);

            AddStep("seek to visible", () => EditorClock.Seek(650));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("slider selected", () => EditorBeatmap.SelectedHitObjects.Single() == slider);
        }

        [Test]
        public void TestContextMenuShownCorrectlyForSelectedSlider()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));

            moveMouseToObject(() => slider);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("slider selected", () => EditorBeatmap.SelectedHitObjects.Single() == slider);

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(blueprintContainer.ChildrenOfType<SliderBodyPiece>().Single().ScreenSpaceDrawQuad.Centre));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
        }

        [Test]
        public void TestSelectionIncludingSliderPreservedOnClick()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(0, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            var secondSlider = new Slider
            {
                StartTime = 1000,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100, -100))
                    }
                }
            };
            var hitCircle = new HitCircle
            {
                StartTime = 200,
                Position = new Vector2(300, 0)
            };

            AddStep("add objects", () => EditorBeatmap.AddRange(new HitObject[] { firstSlider, secondSlider, hitCircle }));
            AddStep("select last 2 objects", () => EditorBeatmap.SelectedHitObjects.AddRange(new HitObject[] { secondSlider, hitCircle }));

            moveMouseToObject(() => secondSlider);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddAssert("selection preserved", () => EditorBeatmap.SelectedHitObjects.Count == 2);
        }

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private ContextMenuContainer contextMenuContainer
            => Editor.ChildrenOfType<ContextMenuContainer>().First();

        private void moveMouseToObject(Func<HitObject> targetFunc)
        {
            AddStep("move mouse to object", () =>
            {
                var pos = blueprintContainer.SelectionBlueprints
                                            .First(s => s.Item == targetFunc())
                                            .ChildrenOfType<HitCirclePiece>()
                                            .First().ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(pos);
            });
        }
    }
}
