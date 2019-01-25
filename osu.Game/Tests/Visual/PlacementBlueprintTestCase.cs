// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit.Compose;

namespace osu.Game.Tests.Visual
{
    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class PlacementBlueprintTestCase : OsuTestCase, IPlacementHandler
    {
        protected readonly Container HitObjectContainer;
        private PlacementBlueprint currentBlueprint;

        protected PlacementBlueprintTestCase()
        {
            Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize = 2;

            Add(HitObjectContainer = CreateHitObjectContainer());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(currentBlueprint = CreateBlueprint());
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IAdjustableClock>(new StopwatchClock());

            return dependencies;
        }

        public void BeginPlacement(HitObject hitObject)
        {
        }

        public void EndPlacement(HitObject hitObject)
        {
            AddHitObject(CreateHitObject(hitObject));

            Remove(currentBlueprint);
            Add(currentBlueprint = CreateBlueprint());
        }

        public void Delete(HitObject hitObject)
        {
        }

        protected virtual Container CreateHitObjectContainer() => new Container { RelativeSizeAxes = Axes.Both };

        protected virtual void AddHitObject(DrawableHitObject hitObject) => HitObjectContainer.Add(hitObject);

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract PlacementBlueprint CreateBlueprint();
    }
}
