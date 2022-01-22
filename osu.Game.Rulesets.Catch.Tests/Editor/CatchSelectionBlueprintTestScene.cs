// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public abstract class CatchSelectionBlueprintTestScene : SelectionBlueprintTestScene
    {
        protected ScrollingHitObjectContainer HitObjectContainer => contentContainer.Playfield.HitObjectContainer;

        protected override Container<Drawable> Content => contentContainer;

        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        protected readonly EditorBeatmap EditorBeatmap;

        private readonly CatchEditorTestSceneContainer contentContainer;

        protected CatchSelectionBlueprintTestScene()
        {
            EditorBeatmap = new EditorBeatmap(new CatchBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new CatchRuleset().RulesetInfo,
                }
            }) { Difficulty = { CircleSize = 0 } };
            EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint
            {
                BeatLength = 100
            });

            base.Content.Add(new EditorBeatmapDependencyContainer(EditorBeatmap, new BindableBeatDivisor())
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    EditorBeatmap,
                    contentContainer = new CatchEditorTestSceneContainer()
                },
            });
        }

        protected void AddMouseMoveStep(double time, float x) => AddStep($"move to time={time}, x={x}", () =>
        {
            float y = HitObjectContainer.PositionAtTime(time);
            Vector2 pos = HitObjectContainer.ToScreenSpace(new Vector2(x, y + HitObjectContainer.DrawHeight));
            InputManager.MoveMouseTo(pos);
        });

        private class EditorBeatmapDependencyContainer : Container
        {
            [Cached]
            private readonly EditorClock editorClock;

            [Cached]
            private readonly BindableBeatDivisor beatDivisor;

            public EditorBeatmapDependencyContainer(IBeatmap beatmap, BindableBeatDivisor beatDivisor)
            {
                editorClock = new EditorClock(beatmap, beatDivisor);
                this.beatDivisor = beatDivisor;
            }
        }
    }
}
