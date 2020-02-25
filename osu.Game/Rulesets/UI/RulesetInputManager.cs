// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        public event Func<T?, List<T>, bool> HandleBindings
        {
            add => ((RulesetKeyBindingContainer)KeyBindingContainer).HandleBindings += value;
            remove => ((RulesetKeyBindingContainer)KeyBindingContainer).HandleBindings -= value;
        }

        public void RetractLastBlocked() => ((RulesetKeyBindingContainer)KeyBindingContainer).RetractLastBlocked();

        protected RulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
        {
            InternalChild = KeyBindingContainer =
                CreateKeyBindingContainer(ruleset, variant, unique)
                    .WithChild(content = new Container { RelativeSizeAxes = Axes.Both });
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
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

        public void Attach(KeyCounterDisplay keyCounter)
        {
            var receptor = new ActionReceptor(keyCounter);

            KeyBindingContainer.Add(receptor);

            keyCounter.SetReceptor(receptor);
            keyCounter.AddRange(KeyBindingContainer.DefaultKeyBindings.Select(b => b.GetAction<T>()).Distinct().Select(b => new KeyCounterAction<T>(b)));
        }

        public class ActionReceptor : KeyCounterDisplay.Receptor, IKeyBindingHandler<T>
        {
            public ActionReceptor(KeyCounterDisplay target)
                : base(target)
            {
            }

            public bool OnPressed(T action) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnPressed(action, Clock.Rate >= 0));

            public void OnReleased(T action)
            {
                foreach (var c in Target.Children.OfType<KeyCounterAction<T>>())
                    c.OnReleased(action, Clock.Rate >= 0);
            }
        }

        #endregion

        protected virtual RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new RulesetKeyBindingContainer(ruleset, variant, unique);

        public class RulesetKeyBindingContainer : DatabasedKeyBindingContainer<T>
        {
            public event Func<T?, List<T>, bool> HandleBindings;
            private UIEvent lastBlocked;

            public RulesetKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!(e is KeyDownEvent || e is MouseDownEvent || e is JoystickButtonEvent)) return base.Handle(e);
                if (e is KeyDownEvent ev && ev.Repeat) return base.Handle(e);

                var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                var combos = KeyBindings?.ToList().FindAll(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any));

                if (combos != null)
                {
                    var pressedActions = combos.Select(c => c.GetAction<T>()).ToList();

                    InputKey? key = null;
                    if (e is KeyDownEvent kb)
                        key = KeyCombination.FromKey(kb.Key);
                    else if (e is MouseDownEvent mouse)
                        key = KeyCombination.FromMouseButton(mouse.Button);
                    else if (e is JoystickButtonEvent joystick)
                        key = KeyCombination.FromJoystickButton(joystick.Button);

                    var single = combos.Find(c => c.KeyCombination.Keys.Any(k => k == key))?.GetAction<T>();

                    var blocked = HandleBindings?.GetInvocationList().Cast<Func<T?, List<T>, bool>>().ToList().FindAll(f => f.Invoke(single, pressedActions)).Any();

                    if (blocked == true)
                    {
                        lastBlocked = e;
                        return false;
                    }

                    lastBlocked = null;
                }

                return base.Handle(e);
            }

            public void RetractLastBlocked()
            {
                if (lastBlocked != null)
                    base.Handle(lastBlocked);
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
    /// Supports attaching a <see cref="KeyCounterDisplay"/>.
    /// Keys will be populated automatically and a receptor will be injected inside.
    /// </summary>
    public interface ICanAttachKeyCounter
    {
        void Attach(KeyCounterDisplay keyCounter);
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
