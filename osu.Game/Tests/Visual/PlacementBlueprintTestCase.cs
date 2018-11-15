// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private readonly Container hitObjectContainer;
        private PlacementBlueprint currentBlueprint;

        protected PlacementBlueprintTestCase()
        {
            Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize = 2;

            Add(hitObjectContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Clock = new FramedClock(new StopwatchClock())
            });
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
            hitObjectContainer.Add(CreateHitObject(hitObject));

            Remove(currentBlueprint);
            Add(currentBlueprint = CreateBlueprint());
        }

        public void Delete(HitObject hitObject)
        {
        }

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract PlacementBlueprint CreateBlueprint();
    }
}
