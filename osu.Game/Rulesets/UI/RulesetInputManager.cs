// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Input.Handlers;
using osu.Game.Screens.Play;
using OpenTK.Input;

namespace osu.Game.Rulesets.UI
{
    public abstract class RulesetInputManager<T> : PassThroughInputManager, ICanAttachKeyCounter, IHasReplayHandler
        where T : struct
    {
        public class RulesetKeyBindingContainer : DatabasedKeyBindingContainer<T>
        {
            public RulesetKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }
        }

        protected readonly KeyBindingContainer<T> KeyBindingContainer;

        protected override Container<Drawable> Content => KeyBindingContainer;

        protected RulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
        {
            InternalChild = KeyBindingContainer = new RulesetKeyBindingContainer(ruleset, variant, unique);
        }

        #region Action mapping (for replays)

        private List<T> lastPressedActions = new List<T>();

        protected override void HandleNewState(InputState state)
        {
            base.HandleNewState(state);

            var replayState = state as ReplayInputHandler.ReplayState<T>;

            if (replayState == null) return;

            // Here we handle states specifically coming from a replay source.
            // These have extra action information rather than keyboard keys or mouse buttons.

            List<T> newActions = replayState.PressedActions;

            foreach (var released in lastPressedActions.Except(newActions))
                KeyBindingContainer.TriggerReleased(released);

            foreach (var pressed in newActions.Except(lastPressedActions))
                KeyBindingContainer.TriggerPressed(pressed);

            lastPressedActions = newActions;
        }

        #endregion

        #region IHasReplayHandler

        private ReplayInputHandler replayInputHandler;
        public ReplayInputHandler ReplayInputHandler
        {
            get
            {
                return replayInputHandler;
            }
            set
            {
                if (replayInputHandler != null) RemoveHandler(replayInputHandler);

                replayInputHandler = value;
                UseParentState = replayInputHandler == null;

                if (replayInputHandler != null)
                    AddHandler(replayInputHandler);
            }
        }

        #endregion

        #region Clock control

        private ManualClock clock;
        private IFrameBasedClock parentClock;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //our clock will now be our parent's clock, but we want to replace this to allow manual control.
            parentClock = Clock;

            ProcessCustomClock = false;
            Clock = new FramedClock(clock = new ManualClock
            {
                CurrentTime = parentClock.CurrentTime,
                Rate = parentClock.Rate,
            });
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

        private bool isAttached => replayInputHandler != null && !UseParentState;

        private const int max_catch_up_updates_per_frame = 50;

        public override bool UpdateSubTree()
        {
            requireMoreUpdateLoops = true;
            validState = true;

            int loops = 0;

            while (validState && requireMoreUpdateLoops && loops++ < max_catch_up_updates_per_frame)
            {
                if (!base.UpdateSubTree())
                    return false;

                UpdateSubTreeMasking(this, ScreenSpaceDrawQuad.AABBFloat);

                if (isAttached)
                {
                    // When handling replay input, we need to consider the possibility of fast-forwarding, which may cause the clock to be updated
                    // to a point very far into the future, then playing a frame at that time. In such a case, lifetime MUST be updated before
                    // input is handled. This is why base.Update is not called from the derived Update when handling replay input, and is instead
                    // called manually at the correct time here.
                    base.Update();
                }
            }

            return true;
        }

        protected override void Update()
        {
            if (parentClock == null) return;

            clock.Rate = parentClock.Rate;
            clock.IsRunning = parentClock.IsRunning;

            if (!isAttached)
            {
                clock.CurrentTime = parentClock.CurrentTime;
            }
            else
            {
                double? newTime = replayInputHandler.SetFrameFromTime(parentClock.CurrentTime);

                if (newTime == null)
                {
                    // we shouldn't execute for this time value. probably waiting on more replay data.
                    validState = false;
                    return;
                }

                clock.CurrentTime = newTime.Value;
            }

            requireMoreUpdateLoops = clock.CurrentTime != parentClock.CurrentTime;

            // The manual clock time has changed in the above code. The framed clock now needs to be updated
            // to ensure that the its time is valid for our children before input is processed
            Clock.ProcessFrame();

            if (!isAttached)
            {
                // For non-replay input handling, this provides equivalent input ordering as if Update was not overridden
                base.Update();
            }
        }

        #endregion

        #region Setting application (disables etc.)

        private Bindable<bool> mouseDisabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
        }

        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            // we don't want to transform the state if a replay is present (for now, at least).
            if (replayInputHandler != null) return;

            var mouse = state.Mouse as Framework.Input.MouseState;

            if (mouse != null)
            {
                if (mouseDisabled.Value)
                {
                    mouse.SetPressed(MouseButton.Left, false);
                    mouse.SetPressed(MouseButton.Right, false);
                }
            }
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
}
