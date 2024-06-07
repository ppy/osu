// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneHitObjectComposer : EditorClockTestScene
    {
        private OsuHitObjectComposer hitObjectComposer;
        private EditorBeatmapContainer editorBeatmapContainer;

        private EditorBeatmap editorBeatmap => editorBeatmapContainer.EditorBeatmap;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap
                {
                    BeatmapInfo =
                    {
                        Ruleset = new OsuRuleset().RulesetInfo
                    },
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(256, 192), Scale = 0.5f
                        },
                        new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                        new Slider
                        {
                            Position = new Vector2(128, 256),
                            Path = new SliderPath(PathType.LINEAR, new[]
                            {
                                Vector2.Zero,
                                new Vector2(216, 0),
                            }),
                            Scale = 0.5f,
                        }
                    },
                });
            });

            AddStep("Create composer", () =>
            {
                Child = editorBeatmapContainer = new EditorBeatmapContainer(Beatmap.Value)
                {
                    Child = hitObjectComposer = new OsuHitObjectComposer(new OsuRuleset())
                    {
                        // force the composer to fully overlap the playfield area by setting a 4:3 aspect ratio.
                        FillMode = FillMode.Fit,
                        FillAspectRatio = 4 / 3f
                    }
                };
            });
        }

        [Test]
        public void TestPlacementOnlyWorksWithTiming()
        {
            AddStep("clear all control points", () => editorBeatmap.ControlPointInfo.Clear());

            AddAssert("Tool is selection", () => hitObjectComposer.ChildrenOfType<ComposeBlueprintContainer>().First().CurrentTool is SelectTool);
            AddAssert("Hitcircle button not clickable", () => !hitObjectComposer.ChildrenOfType<EditorRadioButton>().First(d => d.Button.Label == "HitCircle").Enabled.Value);
            AddStep("Add timing point", () => editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));
            AddAssert("Hitcircle button is clickable", () => hitObjectComposer.ChildrenOfType<EditorRadioButton>().First(d => d.Button.Label == "HitCircle").Enabled.Value);
            AddStep("Change to hitcircle", () => hitObjectComposer.ChildrenOfType<EditorRadioButton>().First(d => d.Button.Label == "HitCircle").TriggerClick());
            AddAssert("Tool changed", () => hitObjectComposer.ChildrenOfType<ComposeBlueprintContainer>().First().CurrentTool is HitCircleCompositionTool);
        }

        [Test]
        public void TestPlacementFailsWhenClickingButton()
        {
            AddStep("clear all control points and hitobjects", () =>
            {
                editorBeatmap.ControlPointInfo.Clear();
                editorBeatmap.Clear();
            });

            AddStep("Add timing point", () => editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));

            AddStep("Change to hitcircle", () => hitObjectComposer.ChildrenOfType<EditorRadioButton>().First(d => d.Button.Label == "HitCircle").TriggerClick());

            ExpandingToolboxContainer toolboxContainer = null!;

            AddStep("move mouse to toolbox", () => InputManager.MoveMouseTo(toolboxContainer = hitObjectComposer.ChildrenOfType<ExpandingToolboxContainer>().First()));
            AddUntilStep("toolbox is expanded", () => toolboxContainer.Expanded.Value);
            AddUntilStep("wait for toolbox to expand", () => toolboxContainer.LatestTransformEndTime, () => Is.EqualTo(Time.Current));

            AddStep("move mouse to overlapping toggle button", () =>
            {
                var playfield = hitObjectComposer.Playfield.ScreenSpaceDrawQuad;
                var button = toolboxContainer.ChildrenOfType<DrawableTernaryButton>().First(b => playfield.Contains(getOverlapPoint(b)));

                InputManager.MoveMouseTo(getOverlapPoint(button));
            });

            AddAssert("no circles placed", () => editorBeatmap.HitObjects.Count == 0);

            AddStep("attempt place circle", () => InputManager.Click(MouseButton.Left));

            AddAssert("no circles placed", () => editorBeatmap.HitObjects.Count == 0);

            Vector2 getOverlapPoint(DrawableTernaryButton ternaryButton)
            {
                var quad = ternaryButton.ScreenSpaceDrawQuad;
                return quad.TopLeft + new Vector2(quad.Width * 9 / 10, quad.Height / 2);
            }
        }

        [Test]
        public void TestPlacementWithinToolboxScrollArea()
        {
            AddStep("clear all control points and hitobjects", () =>
            {
                editorBeatmap.ControlPointInfo.Clear();
                editorBeatmap.Clear();
            });

            AddStep("Add timing point", () => editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));

            AddStep("Change to hitcircle", () => hitObjectComposer.ChildrenOfType<EditorRadioButton>().First(d => d.Button.Label == "HitCircle").TriggerClick());

            AddStep("move mouse to scroll area", () =>
            {
                // Specifically wanting to test the area of overlap between the left expanding toolbox container
                // and the playfield/composer.
                var scrollArea = hitObjectComposer.ChildrenOfType<ExpandingToolboxContainer>().First().ScreenSpaceDrawQuad;
                var playfield = hitObjectComposer.Playfield.ScreenSpaceDrawQuad;
                InputManager.MoveMouseTo(new Vector2(scrollArea.TopLeft.X + 1, playfield.Centre.Y));
            });

            AddAssert("no circles placed", () => editorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestDistanceSpacingHotkeys()
        {
            double originalSpacing = 0;

            AddStep("retrieve original spacing", () => originalSpacing = editorBeatmap.BeatmapInfo.DistanceSpacing);

            AddStep("hold ctrl", () => InputManager.PressKey(Key.LControl));
            AddStep("hold alt", () => InputManager.PressKey(Key.LAlt));

            AddStep("scroll mouse 5 steps", () => InputManager.ScrollVerticalBy(5));

            AddStep("release alt", () => InputManager.ReleaseKey(Key.LAlt));
            AddStep("release ctrl", () => InputManager.ReleaseKey(Key.LControl));

            AddAssert("distance spacing increased by 0.5", () => editorBeatmap.BeatmapInfo.DistanceSpacing == originalSpacing + 0.5);
        }

        public partial class EditorBeatmapContainer : PopoverContainer
        {
            private readonly IWorkingBeatmap working;

            public EditorBeatmap EditorBeatmap { get; private set; }

            public EditorBeatmapContainer(IWorkingBeatmap working)
            {
                this.working = working;

                RelativeSizeAxes = Axes.Both;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

                EditorBeatmap = new EditorBeatmap(working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo));

                dependencies.CacheAs(EditorBeatmap);
                dependencies.CacheAs<IBeatSnapProvider>(EditorBeatmap);

                return dependencies;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Add(EditorBeatmap);
            }
        }
    }
}
