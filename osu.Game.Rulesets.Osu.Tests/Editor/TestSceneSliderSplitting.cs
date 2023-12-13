// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Audio;
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
    public partial class TestSceneSliderSplitting : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private Slider? slider;
        private PathControlPointVisualiser<Slider>? visualiser;

        private const double split_gap = 100;

        [Test]
        public void TestBasicSplit()
        {
            double endTime = 0;

            AddStep("add slider", () =>
            {
                slider = new Slider
                {
                    Position = new Vector2(0, 50),
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero, PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(150, 150)),
                        new PathControlPoint(new Vector2(300, 0), PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(400, 0)),
                        new PathControlPoint(new Vector2(400, 150))
                    })
                };

                EditorBeatmap.Add(slider);

                endTime = slider.EndTime;
            });

            AddStep("select added slider", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(slider);
                visualiser = blueprintContainer.SelectionBlueprints.First(o => o.Item == slider).ChildrenOfType<PathControlPointVisualiser<Slider>>().First();
            });

            moveMouseToControlPoint(2);
            AddStep("select control point", () =>
            {
                if (visualiser is not null) visualiser.Pieces[2].IsSelected.Value = true;
            });
            addContextMenuItemStep("Split control point");

            AddAssert("slider split", () => slider is not null && EditorBeatmap.HitObjects.Count == 2 &&
                                            sliderCreatedFor((Slider)EditorBeatmap.HitObjects[0], 0, EditorBeatmap.HitObjects[1].StartTime - split_gap,
                                                (new Vector2(0, 50), PathType.PERFECT_CURVE),
                                                (new Vector2(150, 200), null),
                                                (new Vector2(300, 50), null)
                                            ) && sliderCreatedFor((Slider)EditorBeatmap.HitObjects[1], slider.StartTime, endTime + split_gap,
                                                (new Vector2(300, 50), PathType.PERFECT_CURVE),
                                                (new Vector2(400, 50), null),
                                                (new Vector2(400, 200), null)
                                            ));

            AddStep("undo", () => Editor.Undo());
            AddAssert("original slider restored", () => EditorBeatmap.HitObjects.Count == 1 && sliderCreatedFor((Slider)EditorBeatmap.HitObjects[0], 0, endTime,
                (new Vector2(0, 50), PathType.PERFECT_CURVE),
                (new Vector2(150, 200), null),
                (new Vector2(300, 50), PathType.PERFECT_CURVE),
                (new Vector2(400, 50), null),
                (new Vector2(400, 200), null)
            ));
        }

        [Test]
        public void TestDoubleSplit()
        {
            double endTime = 0;

            AddStep("add slider", () =>
            {
                slider = new Slider
                {
                    Position = new Vector2(0, 50),
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero, PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(150, 150)),
                        new PathControlPoint(new Vector2(300, 0), PathType.BEZIER),
                        new PathControlPoint(new Vector2(400, 0)),
                        new PathControlPoint(new Vector2(400, 150), PathType.CATMULL),
                        new PathControlPoint(new Vector2(300, 200)),
                        new PathControlPoint(new Vector2(400, 250))
                    })
                };

                EditorBeatmap.Add(slider);

                endTime = slider.EndTime;
            });

            AddStep("select added slider", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(slider);
                visualiser = blueprintContainer.SelectionBlueprints.First(o => o.Item == slider).ChildrenOfType<PathControlPointVisualiser<Slider>>().First();
            });

            moveMouseToControlPoint(2);
            AddStep("select first control point", () =>
            {
                if (visualiser is not null) visualiser.Pieces[2].IsSelected.Value = true;
            });
            moveMouseToControlPoint(4);
            AddStep("select second control point", () =>
            {
                if (visualiser is not null) visualiser.Pieces[4].IsSelected.Value = true;
            });
            addContextMenuItemStep("Split 2 control points");

            AddAssert("slider split", () => slider is not null && EditorBeatmap.HitObjects.Count == 3 &&
                                            sliderCreatedFor((Slider)EditorBeatmap.HitObjects[0], 0, EditorBeatmap.HitObjects[1].StartTime - split_gap,
                                                (new Vector2(0, 50), PathType.PERFECT_CURVE),
                                                (new Vector2(150, 200), null),
                                                (new Vector2(300, 50), null)
                                            ) && sliderCreatedFor((Slider)EditorBeatmap.HitObjects[1], EditorBeatmap.HitObjects[0].GetEndTime() + split_gap, slider.StartTime - split_gap,
                                                (new Vector2(300, 50), PathType.BEZIER),
                                                (new Vector2(400, 50), null),
                                                (new Vector2(400, 200), null)
                                            ) && sliderCreatedFor((Slider)EditorBeatmap.HitObjects[2], EditorBeatmap.HitObjects[1].GetEndTime() + split_gap, endTime + split_gap * 2,
                                                (new Vector2(400, 200), PathType.CATMULL),
                                                (new Vector2(300, 250), null),
                                                (new Vector2(400, 300), null)
                                            ));
        }

        [Test]
        public void TestSplitRetainsHitsounds()
        {
            HitSampleInfo? sample = null;

            AddStep("add slider", () =>
            {
                slider = new Slider
                {
                    Position = new Vector2(0, 50),
                    Path = new SliderPath(new[]
                    {
                        new PathControlPoint(Vector2.Zero, PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(150, 150)),
                        new PathControlPoint(new Vector2(300, 0), PathType.PERFECT_CURVE),
                        new PathControlPoint(new Vector2(400, 0)),
                        new PathControlPoint(new Vector2(400, 150))
                    })
                };

                EditorBeatmap.Add(slider);
            });

            AddStep("add hitsounds", () =>
            {
                if (slider is null) return;

                sample = new HitSampleInfo("hitwhistle", HitSampleInfo.BANK_SOFT, volume: 70);
                slider.Samples.Add(sample.With());
            });

            AddStep("select added slider", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(slider);
                visualiser = blueprintContainer.SelectionBlueprints.First(o => o.Item == slider).ChildrenOfType<PathControlPointVisualiser<Slider>>().First();
            });

            moveMouseToControlPoint(2);
            AddStep("select control point", () =>
            {
                if (visualiser is not null) visualiser.Pieces[2].IsSelected.Value = true;
            });
            addContextMenuItemStep("Split control point");
            AddAssert("sliders have hitsounds", hasHitsounds);

            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[0]));
            AddStep("remove first slider", () => EditorBeatmap.RemoveAt(0));
            AddStep("undo", () => Editor.Undo());
            AddAssert("sliders have hitsounds", hasHitsounds);

            bool hasHitsounds() => sample is not null &&
                                   EditorBeatmap.HitObjects.All(o => o.Samples.Contains(sample));
        }

        private bool sliderCreatedFor(Slider s, double startTime, double endTime, params (Vector2 pos, PathType? pathType)[] expectedControlPoints)
        {
            if (!Precision.AlmostEquals(s.StartTime, startTime, 1) || !Precision.AlmostEquals(s.EndTime, endTime, 1)) return false;

            int i = 0;

            foreach ((Vector2 pos, PathType? pathType) in expectedControlPoints)
            {
                var controlPoint = s.Path.ControlPoints[i++];

                if (!Precision.AlmostEquals(controlPoint.Position + s.Position, pos) || controlPoint.Type != pathType)
                    return false;
            }

            return true;
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                if (slider is null || visualiser is null) return;

                Vector2 position = slider.Path.ControlPoints[index].Position + slider.Position;
                InputManager.MoveMouseTo(visualiser.Pieces[0].Parent!.ToScreenSpace(position));
            });
        }

        private void addContextMenuItemStep(string contextMenuText)
        {
            AddStep($"click context menu item \"{contextMenuText}\"", () =>
            {
                if (visualiser is null) return;

                MenuItem? item = visualiser.ContextMenuItems?.FirstOrDefault(menuItem => menuItem.Text.Value == contextMenuText);

                item?.Action.Value?.Invoke();
            });
        }
    }
}
