// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;

namespace osu.Game.Tests.Visual
{
    public abstract partial class PlacementBlueprintTestScene : OsuManualInputManagerTestScene, IPlacementHandler
    {
        protected readonly Container HitObjectContainer;
        protected HitObjectPlacementBlueprint CurrentBlueprint { get; private set; }

        protected PlacementBlueprintTestScene()
        {
            base.Content.Add(HitObjectContainer = CreateHitObjectContainer().With(c => c.Clock = new FramedClock(new StopwatchClock())));
            base.Content.Add(new MouseMovementInterceptor
            {
                MouseMoved = UpdatePlacementTimeAndPosition,
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
            var rulesetInfo = CreateRuleset()!.RulesetInfo;
            var playable = Beatmap.Value.GetPlayableBeatmap(rulesetInfo);
            playable.BeatmapInfo.Ruleset = rulesetInfo;
            playable.Difficulty.CircleSize = 2;
            return playable;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ResetPlacement();
        }

        public void ShowPlacement(HitObject hitObject)
        {
        }

        public void HidePlacement()
        {
        }

        public void CommitPlacement(HitObject hitObject)
        {
            AddHitObject(CreateHitObject(hitObject));
        }

        protected void ResetPlacement()
        {
            if (CurrentBlueprint != null)
                Remove(CurrentBlueprint, true);
            Add(CurrentBlueprint = CreateBlueprint());
        }

        public void Delete(HitObject hitObject)
        {
        }

        protected override void Update()
        {
            base.Update();

            if (CurrentBlueprint.PlacementActive == PlacementBlueprint.PlacementState.Finished)
                ResetPlacement();

            UpdatePlacementTimeAndPosition();
        }

        protected virtual void UpdatePlacementTimeAndPosition() => CurrentBlueprint.UpdateTimeAndPosition(InputManager.CurrentState.Mouse.Position, 0);

        public override void Add(Drawable drawable)
        {
            base.Add(drawable);

            if (drawable is HitObjectPlacementBlueprint blueprint)
            {
                blueprint.Show();
                UpdatePlacementTimeAndPosition();
            }
        }

        protected virtual void AddHitObject(DrawableHitObject hitObject) => HitObjectContainer.Add(hitObject);

        protected virtual Container CreateHitObjectContainer() => new Container { RelativeSizeAxes = Axes.Both };

        protected abstract DrawableHitObject CreateHitObject(HitObject hitObject);
        protected abstract HitObjectPlacementBlueprint CreateBlueprint();

        private partial class MouseMovementInterceptor : Drawable
        {
            public Action MouseMoved;

            public MouseMovementInterceptor()
            {
                RelativeSizeAxes = Axes.Both;
                Depth = float.MinValue;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                MouseMoved?.Invoke();
                return base.OnMouseMove(e);
            }
        }
    }
}
