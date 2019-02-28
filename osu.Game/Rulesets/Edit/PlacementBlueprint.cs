// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract class PlacementBlueprint : CompositeDrawable, IStateful<PlacementState>
    {
        /// <summary>
        /// Invoked when <see cref="State"/> has changed.
        /// </summary>
        public event Action<PlacementState> StateChanged;

        /// <summary>
        /// Whether the <see cref="HitObject"/> is currently being placed, but has not necessarily finished being placed.
        /// </summary>
        public bool PlacementBegun { get; private set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        protected readonly HitObject HitObject;

        protected IClock EditorClock { get; private set; }

        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

        protected PlacementBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.Both;

            // This is required to allow the blueprint's position to be updated via OnMouseMove/Handle
            // on the same frame it is made visible via a PlacementState change.
            AlwaysPresent = true;

            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, IAdjustableClock clock)
        {
            this.beatmap.BindTo(beatmap);

            EditorClock = clock;

            ApplyDefaultsToHitObject();
        }

        private PlacementState state;

        public PlacementState State
        {
            get => state;
            set
            {
                if (state == value)
                    return;

                state = value;

                if (state == PlacementState.Shown)
                    Show();
                else
                    Hide();

                StateChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has started.
        /// </summary>
        protected void BeginPlacement()
        {
            placementHandler.BeginPlacement(HitObject);
            PlacementBegun = true;
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has finished.
        /// This will destroy this <see cref="PlacementBlueprint"/>, and add the <see cref="HitObject"/> to the <see cref="Beatmap"/>.
        /// </summary>
        protected void EndPlacement()
        {
            if (!PlacementBegun)
                BeginPlacement();
            placementHandler.EndPlacement(HitObject);
        }

        /// <summary>
        /// Invokes <see cref="HitObject.ApplyDefaults"/>, refreshing <see cref="HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.Value.Beatmap.ControlPointInfo, beatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;

        protected override bool Handle(UIEvent e)
        {
            base.Handle(e);

            switch (e)
            {
                case ScrollEvent _:
                    return false;
                case MouseEvent _:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum PlacementState
    {
        Hidden,
        Shown,
    }
}
