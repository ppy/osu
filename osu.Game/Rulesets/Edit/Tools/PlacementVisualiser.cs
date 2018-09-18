// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Tools
{
    public abstract class PlacementVisualiser : CompositeDrawable, IRequireHighFrequencyMousePosition
    {
        public event Action<HitObject> PlacementFinished;

        public HitObject HitObject { get; private set; }

        protected IBeatmap Beatmap { get; private set; }

        private IAdjustableClock clock;

        private double? lastAppliedTime;

        protected PlacementVisualiser(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap workingBeatmap, IAdjustableClock clock)
        {
            this.clock = clock;

            Beatmap = workingBeatmap.Value.Beatmap;
        }

        protected override void Update()
        {
            base.Update();

            if (clock.CurrentTime != lastAppliedTime)
            {
                ApplyTime(clock.CurrentTime);
                HitObject.ApplyDefaults(Beatmap.ControlPointInfo, Beatmap.BeatmapInfo.BaseDifficulty);

                lastAppliedTime = clock.CurrentTime;
            }
        }

        // Todo: Framework bug, this should not nullref
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => Parent?.ReceiveMouseInputAt(screenSpacePos) ?? false;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;
        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;
        protected override bool OnClick(InputState state) => true;
        protected override bool OnDragStart(InputState state) => true;
        protected override bool OnDrag(InputState state) => true;
        protected override bool OnDragEnd(InputState state) => true;

        protected override bool OnMouseMove(InputState state)
        {
            ApplyPosition(state.Mouse.Position);
            return true;
        }

        /// <summary>
        /// Applies a time value to the <see cref="HitObject"/>. This adjusts the start time of <see cref="HitObject"/> by default.
        /// </summary>
        /// <param name="time">The time to apply.</param>
        protected virtual void ApplyTime(double time) => HitObject.StartTime = time;

        /// <summary>
        /// Applies a position value to the <see cref="HitObject"/>. This adjust the position of <see cref="HitObject"/> if it implements <see cref="IHasEditablePosition"/> by default.
        /// </summary>
        /// <param name="position">The position to apply/</param>
        protected virtual void ApplyPosition(Vector2 position)
        {
            if (HitObject is IHasEditablePosition editablePosition)
                editablePosition.Position = position;
        }

        /// <summary>
        /// Invoke to finish placement of <see cref="HitObject"/> and add it to the <see cref="Beatmap"/>.
        /// </summary>
        protected void Finish() => PlacementFinished?.Invoke(HitObject);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            PlacementFinished = null;
        }
    }
}
