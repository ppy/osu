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
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Framework.Input.States;
using osu.Game.Configuration;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;
using osuTK;
using static osu.Game.Input.Handlers.ReplayInputHandler;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class RulesetInputManager<T> : PassThroughInputManager, ICanAttachHUDPieces, IHasReplayHandler, IHasRecordingHandler
        where T : struct
    {
        protected override bool AllowRightClickFromLongTouch => false;

        public readonly KeyBindingContainer<T> KeyBindingContainer;

        [Resolved]
        private ScoreProcessor? scoreProcessor { get; set; }

        private ReplayRecorder? recorder;

        public ReplayRecorder? Recorder
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
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
            tapsDisabled = config.GetBindable<bool>(OsuSetting.TouchDisableGameplayTaps);
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

        private ReplayInputHandler? replayInputHandler;

        public ReplayInputHandler? ReplayInputHandler
        {
            get => replayInputHandler;
            set
            {
                if (replayInputHandler == value)
                    return;

                if (replayInputHandler != null)
                    RemoveHandler(replayInputHandler);

                // ensures that all replay keys are released, that the last replay state is correctly cleared,
                // and that all user-pressed keys are released, so that the replay handler may trigger them itself
                // setting `UseParentInput` will only sync releases (https://github.com/ppy/osu-framework/blob/17d65f476d51cc5f2aaea818534f8fbac47e5fe6/osu.Framework/Input/PassThroughInputManager.cs#L179-L182)
                new ReplayStateReset().Apply(CurrentState, this);

                replayInputHandler = value;
                UseParentInput = replayInputHandler == null;

                if (replayInputHandler != null)
                    AddHandler(replayInputHandler);
            }
        }

        #endregion

        #region Setting application (disables etc.)

        private Bindable<bool> mouseDisabled = null!;
        private Bindable<bool> tapsDisabled = null!;

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
            if (tapsDisabled.Value)
            {
                // Only propagate positional data when taps are disabled.
                e = new TouchStateChangeEvent(e.State, e.Input, e.Touch, false, e.LastPosition);
            }

            return base.HandleMouseTouchStateChange(e);
        }

        #endregion

        #region Key Counter Attachment

        public void Attach(InputCountController inputCountController)
        {
            var triggers = KeyBindingContainer.DefaultKeyBindings
                                              .Select(b => b.GetAction<T>())
                                              .Distinct()
                                              .Select(action => new KeyCounterActionTrigger<T>(action))
                                              .ToArray();

            KeyBindingContainer.AddRange(triggers);
            inputCountController.AddRange(triggers);
        }

        #endregion

        #region Keys per second Counter Attachment

        public void Attach(ClicksPerSecondController controller) => KeyBindingContainer.Add(new ActionListener(controller));

        private partial class ActionListener : Component, IKeyBindingHandler<T>
        {
            private readonly ClicksPerSecondController controller;

            public ActionListener(ClicksPerSecondController controller)
            {
                this.controller = controller;
            }

            public bool OnPressed(KeyBindingPressEvent<T> e)
            {
                controller.AddInputTimestamp();
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<T> e)
            {
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

                KeyBindings = KeyBindings.Where(static b => RealmKeyBindingStore.CheckValidForGameplay(b.KeyCombination)).ToList();
                RealmKeyBindingStore.ClearDuplicateBindings(KeyBindings);
            }
        }

        private class ReplayStateReset : IInput
        {
            public void Apply(InputState state, IInputStateChangeHandler handler)
            {
                if (!(state is RulesetInputManagerInputState<T> inputState))
                    throw new InvalidOperationException($"{nameof(ReplayState<T>)} should only be applied to a {nameof(RulesetInputManagerInputState<T>)}");

                new MouseButtonInput([], state.Mouse.Buttons).Apply(state, handler);
                new KeyboardKeyInput([], state.Keyboard.Keys).Apply(state, handler);
                new TouchInput(Enum.GetValues<TouchSource>().Select(s => new Touch(s, Vector2.Zero)), false).Apply(state, handler);
                new JoystickButtonInput([], state.Joystick.Buttons).Apply(state, handler);
                new MidiKeyInput(new MidiState(), state.Midi).Apply(state, handler);
                new TabletPenButtonInput([], state.Tablet.PenButtons).Apply(state, handler);
                new TabletAuxiliaryButtonInput([], state.Tablet.AuxiliaryButtons).Apply(state, handler);

                handler.HandleInputStateChange(new ReplayStateChangeEvent<T>(state, this, inputState.LastReplayState?.PressedActions.ToArray() ?? [], []));
                inputState.LastReplayState = null;
            }
        }
    }

    public class RulesetInputManagerInputState<T> : InputState
        where T : struct
    {
        public ReplayState<T>? LastReplayState;

        public RulesetInputManagerInputState(InputState state)
            : base(state)
        {
        }
    }
}
