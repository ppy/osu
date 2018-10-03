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
        /// Invoked when the placement of <see cref="HitObject"/> has finished.
        /// </summary>
        public event Action<HitObject> PlacementFinished;

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        protected readonly HitObject HitObject;

        private IAdjustableClock clock;

        public PlacementMask(HitObject hitObject)
        {
            HitObject = hitObject;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap workingBeatmap, IAdjustableClock clock)
        {
            this.clock = clock;

            HitObject.ApplyDefaults(workingBeatmap.Value.Beatmap.ControlPointInfo, workingBeatmap.Value.Beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Fixes a 1-frame position discrpancy due to the first mouse move event happening in the next frame
            Position = GetContainingInputManager().CurrentState.Mouse.Position;
        }

        /// <summary>
        /// Finishes the placement of <see cref="HitObject"/>.
        /// </summary>
        public void Finish() => PlacementFinished?.Invoke(HitObject);

        protected override void Update()
        {
            base.Update();

            HitObject.StartTime = clock.CurrentTime;
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

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Position = e.MousePosition;
            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            PlacementFinished = null;
        }
    }
}
