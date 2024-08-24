// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
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
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Tests.Visual
{
    public abstract partial class PlacementBlueprintTestScene : OsuManualInputManagerTestScene
    {
        private readonly TestPlacementHandler placementHandler;

        protected readonly Container HitObjectContainer;
        protected PlacementBlueprint CurrentBlueprint => placementHandler.CurrentBlueprint;

        protected PlacementBlueprintTestScene()
        {
            // ensure the placement handler is added beneath the input manager layer, for correct input behaviour.
            var contentContainer = CreateContentContainer();
            contentContainer.Add(HitObjectContainer = new Container { Clock = new FramedClock(new StopwatchClock()) });

            base.Content.Add(placementHandler = new TestPlacementHandler(contentContainer)
            {
                CreateBlueprint = CreateBlueprint,
                SnapForBlueprint = SnapForBlueprint,
                AddHitObject = h => AddHitObject(CreateHitObject(h)),
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            var playable = GetPlayableBeatmap();

            var editorClock = new EditorClock();
            base.Content.Add(editorClock);
            dependencies.CacheAs(editorClock);

            var editorBeatmap = new EditorBeatmap(playable);
            // Not adding to hierarchy as we don't satisfy its dependencies. Probably not good.
            dependencies.CacheAs(editorBeatmap);

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

        protected void ResetPlacement() => placementHandler.ResetPlacement();

        protected virtual SnapResult SnapForBlueprint(PlacementBlueprint blueprint) =>
            new SnapResult(InputManager.CurrentState.Mouse.Position, null);

        protected virtual void AddHitObject(DrawableHitObject hitObject) => HitObjectContainer.Add(hitObject);

        protected virtual Container CreateContentContainer() => new Container { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract PlacementBlueprint CreateBlueprint();

        private partial class TestPlacementHandler : CompositeDrawable, IPlacementHandler
        {
            private readonly Container contentContainer;

            public PlacementBlueprint CurrentBlueprint { get; private set; }

            public Func<PlacementBlueprint> CreateBlueprint;
            public Func<PlacementBlueprint, SnapResult> SnapForBlueprint;
            public Action<HitObject> AddHitObject;

            public TestPlacementHandler(Container contentContainer)
            {
                this.contentContainer = contentContainer;

                RelativeSizeAxes = Axes.Both;
                InternalChildren = new Drawable[]
                {
                    contentContainer,
                    new ComposeBlueprintContainer.PlacementBlueprintPositionUpdater
                    {
                        UpdatePosition = updatePlacementTimeAndPosition
                    }
                };
            }

            public void BeginPlacement(HitObject hitObject)
            {
            }

            public void EndPlacement(HitObject hitObject, bool commit)
            {
                if (commit)
                    AddHitObject(hitObject);

                ResetPlacement();
            }

            public void ResetPlacement()
            {
                if (CurrentBlueprint != null)
                    contentContainer.Remove(CurrentBlueprint, true);
                contentContainer.Add(CurrentBlueprint = CreateBlueprint());
            }

            protected override void Update()
            {
                base.Update();
                updatePlacementTimeAndPosition();
            }

            public void Delete(HitObject hitObject)
            {
            }

            protected override void AddInternal(Drawable drawable)
            {
                base.AddInternal(drawable);

                if (drawable is PlacementBlueprint blueprint)
                {
                    blueprint.Show();
                    blueprint.UpdateTimeAndPosition(SnapForBlueprint(blueprint));
                }
            }

            private void updatePlacementTimeAndPosition() => CurrentBlueprint.UpdateTimeAndPosition(SnapForBlueprint(CurrentBlueprint));
        }
    }
}
