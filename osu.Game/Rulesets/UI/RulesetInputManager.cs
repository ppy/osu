// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges.Events;
using osu.Framework.Input.States;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Input.Handlers;
using osu.Game.Screens.Play;
using osuTK.Input;
using static osu.Game.Input.Handlers.ReplayInputHandler;
using JoystickState = osu.Framework.Input.States.JoystickState;
using KeyboardState = osu.Framework.Input.States.KeyboardState;
using MouseState = osu.Framework.Input.States.MouseState;

namespace osu.Game.Rulesets.UI
{
    public abstract class RulesetInputManager<T> : PassThroughInputManager, ICanAttachKeyCounter, IHasReplayHandler
        where T : struct
    {
        protected override InputState CreateInitialState()
        {
            var state = base.CreateInitialState();
            return new RulesetInputManagerInputState<T>(state.Mouse, state.Keyboard, state.Joystick);
        }

        protected readonly KeyBindingContainer<T> KeyBindingContainer;

        protected override Container<Drawable> Content => KeyBindingContainer;

        protected RulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
        {
            InternalChild = KeyBindingContainer = CreateKeyBindingContainer(ruleset, variant, unique);
            gameplayClock = new GameplayClock(framedClock = new FramedClock(manualClock = new ManualClock()));
        }

        #region Action mapping (for replays)

        public override void HandleInputStateChange(InputStateChangeEvent inputStateChange)
        {
            if (inputStateChange is ReplayStateChangeEvent<T> replayStateChanged)
            {
                foreach (var action in replayStateChanged.ReleasedActions)
                    KeyBindingContainer.TriggerReleased(action);

                foreach (var action in replayStateChanged.PressedActions)
                    KeyBindingContainer.TriggerPressed(action);
            }
            else
            {
                base.HandleInputStateChange(inputStateChange);
            }
        }

        #endregion

        #region IHasReplayHandler

        private ReplayInputHandler replayInputHandler;

        public ReplayInputHandler ReplayInputHandler
        {
            get => replayInputHandler;
            set
            {
                if (replayInputHandler != null) RemoveHandler(replayInputHandler);

                replayInputHandler = value;
                UseParentInput = replayInputHandler == null;

                if (replayInputHandler != null)
                    AddHandler(replayInputHandler);
            }
        }

        #endregion

        #region Clock control

        private readonly ManualClock manualClock;

        private readonly FramedClock framedClock;

        [Cached]
        private GameplayClock gameplayClock;

        private IFrameBasedClock parentGameplayClock;

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, GameplayClock clock)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);

            if (clock != null)
                parentGameplayClock = clock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            setClock();
        }

        /// <summary>
        /// Whether we are running up-to-date with our parent clock.
        /// If not, we will need to keep processing children until we catch up.
        /// </summary>
        private bool requireMoreUpdateLoops;

        /// <summary>
        /// Whether we are in a valid state (ie. should we keep processing children frames).
        /// This should be set to false when the replay is, for instance, waiting for future frames to arrive.
        /// </summary>
        private bool validState;

        protected override bool RequiresChildrenUpdate => base.RequiresChildrenUpdate && validState;

        private bool isAttached => replayInputHandler != null && !UseParentInput;

        private const int max_catch_up_updates_per_frame = 50;

        private const double sixty_frame_time = 1000.0 / 60;

        public override bool UpdateSubTree()
        {
            requireMoreUpdateLoops = true;
            validState = true;

            int loops = 0;

            while (validState && requireMoreUpdateLoops && loops++ < max_catch_up_updates_per_frame)
            {
                updateClock();

                if (validState)
                {
                    base.UpdateSubTree();
                    UpdateSubTreeMasking(this, ScreenSpaceDrawQuad.AABBFloat);
                }
            }

            return true;
        }

        private void updateClock()
        {
            if (parentGameplayClock == null)
                setClock(); // LoadComplete may not be run yet, but we still want the clock.

            validState = true;

            manualClock.Rate = parentGameplayClock.Rate;
            manualClock.IsRunning = parentGameplayClock.IsRunning;

            var newProposedTime = parentGameplayClock.CurrentTime;

            try
            {
                if (Math.Abs(manualClock.CurrentTime - newProposedTime) > sixty_frame_time * 1.2f)
                {
                    newProposedTime = manualClock.Rate > 0
                        ? Math.Min(newProposedTime, manualClock.CurrentTime + sixty_frame_time)
                        : Math.Max(newProposedTime, manualClock.CurrentTime - sixty_frame_time);
                }

                if (!isAttached)
                {
                    manualClock.CurrentTime = newProposedTime;
                }
                else
                {
                    double? newTime = replayInputHandler.SetFrameFromTime(newProposedTime);

                    if (newTime == null)
                    {
                        // we shouldn't execute for this time value. probably waiting on more replay data.
                        validState = false;

                        requireMoreUpdateLoops = true;
                        manualClock.CurrentTime = newProposedTime;
                        return;
                    }

                    manualClock.CurrentTime = newTime.Value;
                }

                requireMoreUpdateLoops = manualClock.CurrentTime != parentGameplayClock.CurrentTime;
            }
            finally
            {
                // The manual clock time has changed in the above code. The framed clock now needs to be updated
                // to ensure that the its time is valid for our children before input is processed
                framedClock.ProcessFrame();
            }
        }

        private void setClock()
        {
            // in case a parent gameplay clock isn't available, just use the parent clock.
            if (parentGameplayClock == null)
                parentGameplayClock = Clock;

            Clock = gameplayClock;
            ProcessCustomClock = false;
        }

        #endregion

        #region Setting application (disables etc.)

        private Bindable<bool> mouseDisabled;

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case MouseDownEvent mouseDown when mouseDown.Button == MouseButton.Left || mouseDown.Button == MouseButton.Right:
                    if (mouseDisabled.Value)
                        return false;

                    break;
                case MouseUpEvent mouseUp:
                    if (!CurrentState.Mouse.IsPressed(mouseUp.Button))
                        return false;

                    break;
            }

            return base.Handle(e);
        }

        #endregion

        #region Key Counter Attachment

        public void Attach(KeyCounterCollection keyCounter)
        {
            var receptor = new ActionReceptor(keyCounter);
            Add(receptor);
            keyCounter.SetReceptor(receptor);

            keyCounter.AddRange(KeyBindingContainer.DefaultKeyBindings.Select(b => b.GetAction<T>()).Distinct().Select(b => new KeyCounterAction<T>(b)));
        }

        public class ActionReceptor : KeyCounterCollection.Receptor, IKeyBindingHandler<T>
        {
            public ActionReceptor(KeyCounterCollection target)
                : base(target)
            {
            }

            public bool OnPressed(T action) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnPressed(action));

            public bool OnReleased(T action) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnReleased(action));
        }

        #endregion

        protected virtual RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new RulesetKeyBindingContainer(ruleset, variant, unique);

        public class RulesetKeyBindingContainer : DatabasedKeyBindingContainer<T>
        {
            public RulesetKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }
        }
    }

    /// <summary>
    /// Expose the <see cref="ReplayInputHandler"/>  in a capable <see cref="InputManager"/>.
    /// </summary>
    public interface IHasReplayHandler
    {
        ReplayInputHandler ReplayInputHandler { get; set; }
    }

    /// <summary>
    /// Supports attaching a <see cref="KeyCounterCollection"/>.
    /// Keys will be populated automatically and a receptor will be injected inside.
    /// </summary>
    public interface ICanAttachKeyCounter
    {
        void Attach(KeyCounterCollection keyCounter);
    }

    public class RulesetInputManagerInputState<T> : InputState
        where T : struct
    {
        public ReplayState<T> LastReplayState;

        public RulesetInputManagerInputState(MouseState mouse = null, KeyboardState keyboard = null, JoystickState joystick = null)
            : base(mouse, keyboard, joystick)
        {
        }
    }
}
