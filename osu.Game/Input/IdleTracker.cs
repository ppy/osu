// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Input
{
    /// <summary>
    /// Track whether the end-user is in an idle state, based on their last interaction with the game.
    /// </summary>
    public class IdleTracker : Container
    {
        private readonly double timeToIdle;
        private readonly bool receiveFullInput;

        private readonly BindableBool isIdle = new BindableBool();

        /// <summary>
        /// Whether the user is currently in an idle state.
        /// </summary>
        public IBindable<bool> IsIdle => isIdle;

        protected InteractionTimeReceptor Receptor;

        /// <summary>
        /// Whether the game can currently enter an idle state.
        /// </summary>
        protected virtual bool AllowIdle => true;

        /// <summary>
        /// Intstantiate a new <see cref="IdleTracker"/>.
        /// </summary>
        /// <param name="timeToIdle">The length in milliseconds until an idle state should be assumed.</param>
        /// <param name="receiveFullInput">Whether inputs outside the draw hierarchy should be received as well. If true, the interaction time receptor would be added to the game itself.</param>
        public IdleTracker(double timeToIdle, bool receiveFullInput = false)
        {
            this.timeToIdle = timeToIdle;
            this.receiveFullInput = receiveFullInput;

            RelativeSizeAxes = Axes.Both;
        }

        [Resolved]
        private OsuGameBase game { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var target = receiveFullInput ? game : this as Container;
            target.Add(Receptor = new InteractionTimeReceptor());
        }

        protected override void Update()
        {
            base.Update();

            isIdle.Value = Receptor.TimeSpentIdle > timeToIdle && AllowIdle;
        }

        /// <summary>
        /// A receptor that updates the interaction time and calculates time spent idling.
        /// </summary>
        protected class InteractionTimeReceptor : Component, IKeyBindingHandler<PlatformAction>, IHandleGlobalKeyboardInput
        {
            private double lastInteractionTime;

            public double TimeSpentIdle => Clock.CurrentTime - lastInteractionTime;

            public InteractionTimeReceptor()
            {
                RelativeSizeAxes = Axes.Both;
            }

            public bool OnPressed(PlatformAction action) => updateLastInteractionTime();

            public bool OnReleased(PlatformAction action) => updateLastInteractionTime();

            protected override bool Handle(UIEvent e)
            {
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
}
