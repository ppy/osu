// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;

namespace osu.Game.Tests.Visual
{
    public abstract class PlacementBlueprintTestScene : OsuManualInputManagerTestScene, IPlacementHandler
    {
        protected readonly Container HitObjectContainer;
        protected PlacementBlueprint CurrentBlueprint { get; private set; }

        protected PlacementBlueprintTestScene()
        {
            base.Content.Add(HitObjectContainer = CreateHitObjectContainer().With(c => c.Clock = new FramedClock(new StopwatchClock())));
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(new EditorClock());

            var playable = GetPlayableBeatmap();
            dependencies.CacheAs(new EditorBeatmap(playable));

            return dependencies;
        }

        protected virtual IBeatmap GetPlayableBeatmap()
        {
            var playable = Beatmap.Value.GetPlayableBeatmap(Beatmap.Value.BeatmapInfo.Ruleset);
            playable.Difficulty.CircleSize = 2;
            return playable;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ResetPlacement();
        }

        public void BeginPlacement(HitObject hitObject)
        {
        }

        public void EndPlacement(HitObject hitObject, bool commit)
        {
            if (commit)
                AddHitObject(CreateHitObject(hitObject));

            ResetPlacement();
        }

        protected void ResetPlacement()
        {
            if (CurrentBlueprint != null)
                Remove(CurrentBlueprint);
            Add(CurrentBlueprint = CreateBlueprint());
        }

        public void Delete(HitObject hitObject)
        {
        }

        protected override void Update()
        {
            base.Update();

            CurrentBlueprint.UpdateTimeAndPosition(SnapForBlueprint(CurrentBlueprint));
        }

        protected virtual SnapResult SnapForBlueprint(PlacementBlueprint blueprint) =>
            new SnapResult(InputManager.CurrentState.Mouse.Position, null);

        public override void Add(Drawable drawable)
        {
            base.Add(drawable);

            if (drawable is PlacementBlueprint blueprint)
            {
                blueprint.Show();
                blueprint.UpdateTimeAndPosition(SnapForBlueprint(blueprint));
            }
        }

        protected virtual void AddHitObject(DrawableHitObject hitObject) => HitObjectContainer.Add(hitObject);

        protected virtual Container CreateHitObjectContainer() => new Container { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract PlacementBlueprint CreateBlueprint();
    }
}
