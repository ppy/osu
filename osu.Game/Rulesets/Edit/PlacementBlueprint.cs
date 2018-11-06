// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Screens.Compose;
using OpenTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract class PlacementBlueprint : CompositeDrawable, IRequireHighFrequencyMousePosition
    {
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
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap, IAdjustableClock clock)
        {
            this.beatmap.BindTo(beatmap);

            EditorClock = clock;

            ApplyDefaultsToHitObject();
        }

        private bool placementBegun;

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has started.
        /// </summary>
        protected void BeginPlacement()
        {
            placementHandler.BeginPlacement(HitObject);
            placementBegun = true;
        }

        /// <summary>
        /// Signals that the placement of <see cref="HitObject"/> has finished.
        /// This will destroy this <see cref="PlacementBlueprint"/>, and add the <see cref="HitObject"/> to the <see cref="Beatmap"/>.
        /// </summary>
        protected void EndPlacement()
        {
            if (!placementBegun)
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
}
