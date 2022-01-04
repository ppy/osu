// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneHitObjectComposer : EditorClockTestScene
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
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle { Position = new Vector2(256, 192), Scale = 0.5f },
                        new HitCircle { Position = new Vector2(344, 148), Scale = 0.5f },
                        new Slider
                        {
                            Position = new Vector2(128, 256),
                            Path = new SliderPath(PathType.Linear, new[]
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

        public class EditorBeatmapContainer : Container
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
