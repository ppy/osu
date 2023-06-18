// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Game.Rulesets.Scoring;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class RulesetInputManager<T> : PassThroughInputManager, IKeybindingEventsEmitter, IHasReplayHandler, IHasRecordingHandler
        where T : struct
    {
        protected override bool AllowRightClickFromLongTouch => false;

        public readonly KeyBindingContainer<T> KeyBindingContainer;

        [Resolved(CanBeNull = true)]
        private ScoreProcessor scoreProcessor { get; set; }

        private ReplayRecorder recorder;

        public ReplayRecorder Recorder
        {
            set
            {
                if (value == recorder)
                    return;

                if (value != null && recorder != null)
                    throw new InvalidOperationException("Cannot attach more than one recorder");

                recorder?.Expire();
                recorder = value;

                if (recorder != null)
                    KeyBindingContainer.Add(recorder);
            }
        }

        protected override InputState CreateInitialState() => new RulesetInputManagerInputState<T>(base.CreateInitialState());

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        protected RulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
        {
            InternalChild = KeyBindingContainer =
                CreateKeyBindingContainer(ruleset, variant, unique)
                    .WithChild(content = new Container { RelativeSizeAxes = Axes.Both });
            KeyBindingContainer.Add(actionListener = new ActionListener());
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
        }

        #region Action mapping (for replays)

        public override void HandleInputStateChange(InputStateChangeEvent inputStateChange)
        {
            switch (inputStateChange)
            {
                case ReplayStateChangeEvent<T> stateChangeEvent:
                    foreach (var action in stateChangeEvent.ReleasedActions)
                        KeyBindingContainer.TriggerReleased(action);

                    foreach (var action in stateChangeEvent.PressedActions)
                        KeyBindingContainer.TriggerPressed(action);
                    break;

                case ReplayStatisticsFrameEvent statisticsStateChangeEvent:
                    scoreProcessor?.ResetFromReplayFrame(statisticsStateChangeEvent.Frame);
                    break;

                default:
                    base.HandleInputStateChange(inputStateChange);
                    break;
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
                case MouseDownEvent:
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

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e)
        {
            if (mouseDisabled.Value)
            {
                // Only propagate positional data when mouse buttons are disabled.
                e = new TouchStateChangeEvent(e.State, e.Input, e.Touch, false, e.LastPosition);
            }

            return base.HandleMouseTouchStateChange(e);
        }

        #endregion

        #region Component attachement

        private readonly ActionListener actionListener;

        public void Attach(IKeybindingListener skinComponent)
        {
            skinComponent.Setup(KeyBindingContainer.DefaultKeyBindings
                                                   .Select(b => b.GetAction<T>())
                                                   .Distinct()
                                                   .OrderBy(a => a));

            if (skinComponent.CanHandleKeybindings && skinComponent is Drawable component)
            {
                try
                {
                    KeyBindingContainer.Add(component);
                    return;
                }
                catch (Exception)
                {
                    return;
                }
            }

            actionListener.OnPressedEvent += skinComponent.OnPressed;
            actionListener.OnReleasedEvent += skinComponent.OnReleased;
        }

        private partial class ActionListener : Component, IKeyBindingHandler<T>
        {
            public event Action<KeyBindingPressEvent<T>> OnPressedEvent;

            public event Action<KeyBindingReleaseEvent<T>> OnReleasedEvent;

            public bool OnPressed(KeyBindingPressEvent<T> e)
            {
                OnPressedEvent?.Invoke(e);
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<T> e)
            {
                OnReleasedEvent?.Invoke(e);
            }
        }

        #endregion

        protected virtual KeyBindingContainer<T> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new RulesetKeyBindingContainer(ruleset, variant, unique);

        public partial class RulesetKeyBindingContainer : DatabasedKeyBindingContainer<T>
        {
            protected override bool HandleRepeats => false;

            public RulesetKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override void ReloadMappings(IQueryable<RealmKeyBinding> realmKeyBindings)
            {
                base.ReloadMappings(realmKeyBindings);

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
    /// Sends <see cref="IKeyBinding"/> events to a <see cref="IKeybindingListener"/>
    /// </summary>
    public interface IKeybindingEventsEmitter
    {
        void Attach(IKeybindingListener component);
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
