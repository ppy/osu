// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Game.Input.Bindings;

namespace osu.Game.Input
{
    /// <summary>
    /// Track whether the end-user is in an idle state, based on their last interaction with the game.
    /// </summary>
    public class IdleTracker : Component, IKeyBindingHandler<PlatformAction>, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        private readonly double timeToIdle;

        private double lastInteractionTime;

        protected double TimeSpentIdle => Clock.CurrentTime - lastInteractionTime;

        /// <summary>
        /// Whether the user is currently in an idle state.
        /// </summary>
        public IBindable<bool> IsIdle => isIdle;

        private readonly BindableBool isIdle = new BindableBool();

        /// <summary>
        /// Whether the game can currently enter an idle state.
        /// </summary>
        protected virtual bool AllowIdle => true;

        /// <summary>
        /// Intstantiate a new <see cref="IdleTracker"/>.
        /// </summary>
        /// <param name="timeToIdle">The length in milliseconds until an idle state should be assumed.</param>
        public IdleTracker(double timeToIdle)
        {
            this.timeToIdle = timeToIdle;
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateLastInteractionTime();
        }

        protected override void Update()
        {
            base.Update();
            isIdle.Value = TimeSpentIdle > timeToIdle && AllowIdle;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e) => updateLastInteractionTime();

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e) => updateLastInteractionTime();

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e) => updateLastInteractionTime();

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) => updateLastInteractionTime();

        [Resolved]
        private GameHost host { get; set; }

        protected override bool Handle(UIEvent e)
        {
            // Even when not active, `MouseMoveEvent`s will arrive.
            // We don't want these to trigger a non-idle state as it's quite often the user interacting
            // with other windows while osu! is in the background.
            if (!host.IsActive.Value)
                return base.Handle(e);

            switch (e)
            {
                case KeyDownEvent _:
                case KeyUpEvent _:
                case MouseDownEvent _:
                case MouseUpEvent _:
                case MouseMoveEvent _:
                    return updateLastInteractionTime();

                default:
                    return base.Handle(e);
            }
        }

        private bool updateLastInteractionTime()
        {
            lastInteractionTime = Clock.CurrentTime;
            return false;
        }
    }
}
