// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Game.Overlays;

namespace osu.Game.Screens.Utility.SampleComponents
{
    public abstract class LatencySampleComponent : CompositeDrawable
    {
        protected readonly BindableBool IsActive = new BindableBool();

        private InputManager inputManager = null!;

        [Resolved]
        private LatencyArea latencyArea { get; set; } = null!;

        [Resolved]
        protected OverlayColourProvider OverlayColourProvider { get; private set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
            IsActive.BindTo(latencyArea.IsActiveArea);
        }

        protected sealed override void Update()
        {
            base.Update();
            UpdateAtLimitedRate(inputManager.CurrentState);
        }

        protected abstract void UpdateAtLimitedRate(InputState inputState);
    }
}
