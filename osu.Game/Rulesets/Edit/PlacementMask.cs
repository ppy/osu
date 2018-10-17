// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Edit
{
    public class PlacementMask : CompositeDrawable, IRequireHighFrequencyMousePosition
    {
        /// <summary>
        /// Invoked when the placement of <see cref="HitObject"/> has started.
        /// </summary>
        public event Action<HitObject> PlacementStarted;

        /// <summary>
        /// Invoked when the placement of <see cref="HitObject"/> has finished.
        /// </summary>
        public event Action<HitObject> PlacementFinished;

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        protected readonly HitObject HitObject;

        protected IClock EditorClock { get; private set; }

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
            PlacementStarted?.Invoke(HitObject);
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
            PlacementFinished?.Invoke(HitObject);
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            PlacementStarted = null;
            PlacementFinished = null;
        }
    }
}
