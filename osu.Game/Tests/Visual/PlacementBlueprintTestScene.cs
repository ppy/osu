// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit.Compose;

namespace osu.Game.Tests.Visual
{
    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class PlacementBlueprintTestScene : OsuTestScene, IPlacementHandler
    {
        protected Container HitObjectContainer;
        private PlacementBlueprint currentBlueprint;

        private InputManager inputManager;

        protected PlacementBlueprintTestScene()
        {
            Add(HitObjectContainer = CreateHitObjectContainer().With(c => c.Clock = new FramedClock(new StopwatchClock())));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value.BeatmapInfo.BaseDifficulty.CircleSize = 2;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IAdjustableClock>(new StopwatchClock());

            return dependencies;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
            Add(currentBlueprint = CreateBlueprint());
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

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            currentBlueprint.UpdatePosition(e.ScreenSpaceMousePosition);
            return true;
        }

        public override void Add(Drawable drawable)
        {
            base.Add(drawable);

            if (drawable is PlacementBlueprint blueprint)
            {
                blueprint.Show();
                blueprint.UpdatePosition(inputManager.CurrentState.Mouse.Position);
            }
        }

        protected virtual void AddHitObject(DrawableHitObject hitObject) => HitObjectContainer.Add(hitObject);

        protected virtual Container CreateHitObjectContainer() => new Container { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract PlacementBlueprint CreateBlueprint();
    }
}
