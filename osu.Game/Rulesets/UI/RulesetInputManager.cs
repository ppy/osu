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
using osu.Game.Configuration;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Input.Handlers;
using osu.Game.Screens.Play;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.UI
{
    public abstract class RulesetInputManager<T> : PassThroughInputManager, ICanAttachKeyCounter, IHasReplayHandler, IHasRecordingHandler
        where T : struct
    {
        private ReplayRecorder recorder;

        public ReplayRecorder Recorder
        {
            set
            {
                if (value != null && recorder != null)
                    throw new InvalidOperationException("Cannot attach more than one recorder");

                recorder?.Expire();
                recorder = value;

                if (recorder != null)
                    KeyBindingContainer.Add(recorder);
            }
        }

        protected override InputState CreateInitialState() => new RulesetInputManagerInputState<T>(base.CreateInitialState());

        protected readonly KeyBindingContainer<T> KeyBindingContainer;

        protected override Container<Drawable> Content => content;

        private readonly Container content;

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
                case MouseDownEvent _:
                    if (mouseDisabled.Value)
                        return true; // importantly, block upwards propagation so global bindings also don't fire.

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
            keyCounter.AddRange(KeyBindingContainer.DefaultKeyBindings
                                                   .Select(b => b.GetAction<T>())
                                                   .Distinct()
                                                   .OrderBy(action => action)
                                                   .Select(action => new KeyCounterAction<T>(action)));
        }

        public class ActionReceptor : KeyCounterDisplay.Receptor, IKeyBindingHandler<T>
        {
            public ActionReceptor(KeyCounterDisplay target)
                : base(target)
            {
            }

            public bool OnPressed(KeyBindingPressEvent<T> e) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnPressed(e.Action, Clock.Rate >= 0));

            public void OnReleased(KeyBindingReleaseEvent<T> e)
            {
                foreach (var c in Target.Children.OfType<KeyCounterAction<T>>())
                    c.OnReleased(e.Action, Clock.Rate >= 0);
            }
        }

        #endregion

        protected virtual KeyBindingContainer<T> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new RulesetKeyBindingContainer(ruleset, variant, unique);

        public class RulesetKeyBindingContainer : DatabasedKeyBindingContainer<T>
        {
            protected override bool HandleRepeats => false;

            public RulesetKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override void ReloadMappings()
            {
                base.ReloadMappings();

                KeyBindings = KeyBindings.Where(b => RealmKeyBindingStore.CheckValidForGameplay(b.KeyCombination)).ToList();
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

    public interface IHasRecordingHandler
    {
        public ReplayRecorder Recorder { set; }
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

        public RulesetInputManagerInputState(InputState state = null)
            : base(state)
        {
        }
    }
}
