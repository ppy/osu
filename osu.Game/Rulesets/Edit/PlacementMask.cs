// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
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
    public class PlacementMask : CompositeDrawable, IRequireHighFrequencyMousePosition
    {
        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        protected readonly HitObject HitObject;

        protected IClock EditorClock { get; private set; }

        [Resolved]
        private IPlacementHandler placementHandler { get; set; }

        public PlacementMask(HitObject hitObject)
        {
            HitObject = hitObject;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap workingBeatmap, IAdjustableClock clock)
        {
            EditorClock = clock;

            HitObject.ApplyDefaults(workingBeatmap.Value.Beatmap.ControlPointInfo, workingBeatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty);
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
        /// This will destroy this <see cref="PlacementMask"/>, and add the <see cref="HitObject"/> to the <see cref="Beatmap"/>.
        /// </summary>
        protected void EndPlacement()
        {
            if (!placementBegun)
                BeginPlacement();
            placementHandler.EndPlacement(HitObject);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parent?.ReceivePositionalInputAt(screenSpacePos) ?? false;

        protected override bool Handle(UIEvent e)
        {
            base.Handle(e);

            switch (e)
            {
                case MouseEvent _:
                    return true;
                default:
                    return false;
            }
        }
    }
}
