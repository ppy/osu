// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Menu
{
    public partial class ButtonSystem : Container, IStateful<ButtonSystemState>, IKeyBindingHandler<GlobalAction>
    {
        public event Action<ButtonSystemState> StateChanged;

        private readonly IBindable<bool> isIdle = new BindableBool();

        public Action OnEdit;
        public Action OnExit;
        public Action OnBeatmapListing;
        public Action OnSolo;
        public Action OnSettings;
        public Action OnMultiplayer;
        public Action OnPlaylists;

        public const float BUTTON_WIDTH = 140f;
        public const float WEDGE_WIDTH = 20;

        [CanBeNull]
        private OsuLogo logo;

        /// <summary>
        /// Assign the <see cref="OsuLogo"/> that this ButtonSystem should manage the position of.
        /// </summary>
        /// <param name="logo">The instance of the logo to be assigned. If null, we are suspending from the screen that uses this ButtonSystem.</param>
        public void SetOsuLogo(OsuLogo logo)
        {
            this.logo = logo;

            if (this.logo != null)
            {
                this.logo.Action = onOsuLogo;

                // osuLogo.SizeForFlow relies on loading to be complete.
                buttonArea.Flow.Position = new Vector2(WEDGE_WIDTH * 2 - (BUTTON_WIDTH + this.logo.SizeForFlow / 4), 0);

                updateLogoState();
            }
            else
            {
                // We should stop tracking as the facade is now out of scope.
                logoTrackingContainer.StopTracking();
            }
        }

        private readonly ButtonArea buttonArea;

        private readonly MainMenuButton backButton;
        private readonly MainMenuButton settingsButton;

        [CanBeNull]
        internal MainMenuButton PlayButton;

        private readonly List<MainMenuButton> buttonsTopLevel = new List<MainMenuButton>();
        private readonly List<MainMenuButton> buttonsPlay = new List<MainMenuButton>();

        [CanBeNull]
        public List<MainMenuButton> CurrentButtonsList
        {
            get
            {
                switch (State)
                {
                    case ButtonSystemState.TopLevel: return buttonsTopLevel;
                    case ButtonSystemState.Play: return buttonsPlay;
                    default: return null;
                }
            }
        }

        private int? selectionIndex = null;

        public int? SelectionIndex
        {
            get => selectionIndex;
            set
            {
                if (selectionIndex == value)
                    return;

                (CurrentSelection as MainMenuButton)?.SimulateHoverLost();
                selectionIndex = value;
                (CurrentSelection as MainMenuButton)?.SimulateHover();
            }
        }

        /// <summary>
        /// The current keyboard-focused MainMenuButton, or the osu logo if
        /// nothing valid is focused.
        /// </summary>
        [CanBeNull]
        public Container CurrentSelection =>
            (SelectionIndex >= 0 && SelectionIndex < CurrentButtonsList?.Count)
                ? CurrentButtonsList[SelectionIndex.Value]
                : logo;

        /// <summary>
        /// When using keyboard navigation, mouse position at last keyboard
        /// input. Reset to Vector2.Zero when hovering over buttons. When a
        /// hover is registered very close to this position, we ignore the hover.
        /// </summary>
        private Vector2? blockedMousePosition = null;

        private Sample sampleBack;

        private readonly LogoTrackingContainer logoTrackingContainer;

        public bool ReturnToTopOnIdle { get; set; } = true;

        public ButtonSystem()
        {
            RelativeSizeAxes = Axes.Both;

            Child = logoTrackingContainer = new LogoTrackingContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = buttonArea = new ButtonArea()
            };

            buttonArea.AddRange(new Drawable[]
            {
                settingsButton = new MainMenuButton(ButtonSystemStrings.Settings, string.Empty, FontAwesome.Solid.Cog, new Color4(85, 85, 85, 255), () => OnSettings?.Invoke(),
                    -WEDGE_WIDTH, Key.O, hoverAction: processMenuButtonHover),
                backButton = new MainMenuButton(ButtonSystemStrings.Back, @"button-back-select", OsuIcon.LeftCircle, new Color4(51, 58, 94, 255), () => State = ButtonSystemState.TopLevel,
                    -WEDGE_WIDTH, hoverAction: processMenuButtonHover)
                {
                    VisibleState = ButtonSystemState.Play,
                },
                logoTrackingContainer.LogoFacade.With(d => d.Scale = new Vector2(0.74f))
            });

            buttonArea.Flow.CentreTarget = logoTrackingContainer.LogoFacade;
        }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(CanBeNull = true)]
        private LoginOverlay loginOverlay { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, IdleTracker idleTracker, GameHost host)
        {
            buttonsPlay.Add(new MainMenuButton(ButtonSystemStrings.Solo, @"button-solo-select", FontAwesome.Solid.User, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(),
                WEDGE_WIDTH, Key.P, hoverAction: processMenuButtonHover));
            buttonsPlay.Add(new MainMenuButton(ButtonSystemStrings.Multi, @"button-generic-select", FontAwesome.Solid.Users, new Color4(94, 63, 186, 255), onMultiplayer,
                0, Key.M, hoverAction: processMenuButtonHover));
            buttonsPlay.Add(new MainMenuButton(ButtonSystemStrings.Playlists, @"button-generic-select", OsuIcon.Charts, new Color4(94, 63, 186, 255), onPlaylists,
                0, Key.L, hoverAction: processMenuButtonHover));
            buttonsPlay.ForEach(b => b.VisibleState = ButtonSystemState.Play);

            buttonsTopLevel.Add(
                PlayButton = new MainMenuButton(ButtonSystemStrings.Play, @"button-play-select", OsuIcon.Logo, new Color4(102, 68, 204, 255), () => State = ButtonSystemState.Play,
                    WEDGE_WIDTH, Key.P, hoverAction: processMenuButtonHover)
            );
            buttonsTopLevel.Add(new MainMenuButton(ButtonSystemStrings.Edit, @"button-edit-select", OsuIcon.EditCircle, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(),
                0, Key.E, hoverAction: processMenuButtonHover));
            buttonsTopLevel.Add(new MainMenuButton(ButtonSystemStrings.Browse, @"button-direct-select", OsuIcon.ChevronDownCircle, new Color4(165, 204, 0, 255), () => OnBeatmapListing?.Invoke(),
                0, Key.D, hoverAction: processMenuButtonHover));

            if (host.CanExit)
            {
                buttonsTopLevel.Add(new MainMenuButton(ButtonSystemStrings.Exit, string.Empty, OsuIcon.CrossCircle, new Color4(238, 51, 153, 255), () => OnExit?.Invoke(),
                    0, Key.Q, hoverAction: processMenuButtonHover));
            }

            buttonArea.AddRange(buttonsPlay);
            buttonArea.AddRange(buttonsTopLevel);

            // for keyboard navigation
            buttonsTopLevel.Insert(0, settingsButton);
            buttonsPlay.Insert(0, backButton);

            buttonArea.ForEach(b =>
            {
                if (b is MainMenuButton)
                {
                    b.Origin = Anchor.CentreLeft;
                    b.Anchor = Anchor.CentreLeft;
                }
            });

            isIdle.ValueChanged += idle => updateIdleState(idle.NewValue);

            if (idleTracker != null) isIdle.BindTo(idleTracker.IsIdle);

            sampleBack = audio.Samples.Get(@"Menu/button-back-select");
        }

        private void onMultiplayer()
        {
            if (api.State.Value != APIState.Online)
            {
                loginOverlay?.Show();
                return;
            }

            OnMultiplayer?.Invoke();
        }

        private void onPlaylists()
        {
            if (api.State.Value != APIState.Online)
            {
                loginOverlay?.Show();
                return;
            }

            OnPlaylists?.Invoke();
        }

        private void updateIdleState(bool isIdle)
        {
            if (!ReturnToTopOnIdle)
                return;

            if (isIdle && State != ButtonSystemState.Exit && State != ButtonSystemState.EnteringMode)
                State = ButtonSystemState.Initial;
        }

        /// <summary>
        /// Triggers the <see cref="logo"/> if the current <see cref="State"/> is <see cref="ButtonSystemState.Initial"/>.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="logo"/> was triggered, <c>false</c> otherwise.</returns>
        private bool triggerInitialOsuLogo()
        {
            if (State == ButtonSystemState.Initial)
            {
                logo?.TriggerClick();
                return true;
            }

            return false;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || e.ControlPressed || e.ShiftPressed || e.AltPressed || e.SuperPressed)
                return false;

            if (triggerInitialOsuLogo())
                return true;

            return base.OnKeyDown(e);
        }

        protected override bool OnJoystickPress(JoystickPressEvent e)
        {
            if (triggerInitialOsuLogo())
                return true;

            return base.OnJoystickPress(e);
        }

        protected override bool OnMidiDown(MidiDownEvent e)
        {
            if (triggerInitialOsuLogo())
                return true;

            return base.OnMidiDown(e);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    PreferKeyboardNavigation();
                    return goBack();

                case GlobalAction.Select:
                case GlobalAction.SelectNext:
                    PreferKeyboardNavigation();
                    CurrentSelection?.TriggerClick();
                    // keyboard navgiation into submenu
                    if (State != ButtonSystemState.EnteringMode)
                        SelectionIndex = 1; // 1 to skip the back/settings btn
                    return true;

                case GlobalAction.SelectPrevious:
                    PreferKeyboardNavigation();
                    SelectUp();
                    return true;

                case GlobalAction.SelectNextGroup:
                    PreferKeyboardNavigation();
                    SelectNext(1);
                    return true;

                case GlobalAction.SelectPreviousGroup:
                    PreferKeyboardNavigation();
                    SelectNext(-1);
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public void SelectNext(int d)
        {
            if (d > 0 && State == ButtonSystemState.Initial)
            {
                State = ButtonSystemState.TopLevel;
                SelectionIndex = 1;
            }
            else if (CurrentButtonsList == null)
            {
                SelectionIndex = null;
            }
            else
            {
                // start at 1, to not focus back/settings button, except when clicking left (focuses on settings)
                int nextSelection = SelectionIndex ?? 0;
                nextSelection += d;

                if (nextSelection >= CurrentButtonsList.Count)
                    nextSelection = CurrentButtonsList.Count - 1;
                else if (nextSelection < 0)
                    nextSelection = 0;

                SelectionIndex = nextSelection;
            }
        }

        public void SelectUp()
        {
            // if we come from play, focus play button again
            int? parentSelection = State == ButtonSystemState.Play ? buttonsTopLevel.FindIndex(b => b == PlayButton) : null;
            if (parentSelection == -1)
                parentSelection = null;
            goBack();
            SelectionIndex = parentSelection;
        }

        private bool processMenuButtonHover(Vector2 screenSpacePosition, [CanBeNull] Drawable hoverElement)
        {
            if (!ConfirmHover(screenSpacePosition))
                return false;

            if (hoverElement == null)
                return true; // unhover event

            SelectionIndex = null;

            if (CurrentButtonsList == null)
                return true;

            foreach (var btn in CurrentButtonsList.Where(btn => btn != hoverElement))
                btn.SimulateHoverLost();
            return true;
        }

        public void PreferKeyboardNavigation()
        {
            blockedMousePosition = GetContainingInputManager().CurrentState.Mouse.Position;

            if (CurrentButtonsList == null)
                return;

            foreach (var btn in CurrentButtonsList.Where(btn => btn != CurrentSelection))
                btn.SimulateHoverLost();
        }

        public bool ConfirmHover(Vector2 currentMousePosition)
        {
            if (blockedMousePosition == null)
            {
                return true;
            }
            else if ((blockedMousePosition.Value - currentMousePosition).LengthSquared > 4 * 4)
            {
                blockedMousePosition = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            ConfirmHover(e.ScreenSpaceMousePosition);
            return base.OnMouseMove(e);
        }

        private bool goBack()
        {
            switch (State)
            {
                case ButtonSystemState.TopLevel:
                    State = ButtonSystemState.Initial;
                    sampleBack?.Play();
                    return true;

                case ButtonSystemState.Play:
                    backButton.TriggerClick();
                    return true;

                default:
                    return false;
            }
        }

        private bool onOsuLogo()
        {
            switch (state)
            {
                default:
                    return false;

                case ButtonSystemState.Initial:
                    State = ButtonSystemState.TopLevel;
                    return true;

                case ButtonSystemState.TopLevel:
                    PlayButton!.TriggerClick();
                    return false;

                case ButtonSystemState.Play:
                    buttonsPlay.First().TriggerClick();
                    return false;
            }
        }

        private ButtonSystemState state = ButtonSystemState.Initial;

        public override bool HandleNonPositionalInput => state != ButtonSystemState.Exit;
        public override bool HandlePositionalInput => state != ButtonSystemState.Exit;

        public ButtonSystemState State
        {
            get => state;

            set
            {
                if (state == value) return;

                ButtonSystemState lastState = state;
                state = value;
                // Clear keyboard selection when it's coming from a click.
                // In case of keyboard navigation we want to keep the selection
                // visible, so this is reassigned immediately afterwards in
                // other places in the code.
                SelectionIndex = null;

                updateLogoState(lastState);

                Logger.Log($"{nameof(ButtonSystem)}'s state changed from {lastState} to {state}");

                using (buttonArea.BeginDelayedSequence(lastState == ButtonSystemState.Initial ? 150 : 0))
                {
                    buttonArea.ButtonSystemState = state;

                    foreach (var b in buttonArea.Children.OfType<MainMenuButton>())
                        b.ButtonSystemState = state;
                }

                StateChanged?.Invoke(State);
            }
        }

        private ScheduledDelegate logoDelayedAction;

        private void updateLogoState(ButtonSystemState lastState = ButtonSystemState.Initial)
        {
            if (logo == null) return;

            switch (state)
            {
                case ButtonSystemState.Exit:
                case ButtonSystemState.Initial:
                    logoDelayedAction?.Cancel();
                    logoDelayedAction = Scheduler.AddDelayed(() =>
                    {
                        logoTrackingContainer.StopTracking();

                        game?.Toolbar.Hide();

                        logo?.ClearTransforms(targetMember: nameof(Position));
                        logo?.MoveTo(new Vector2(0.5f), 800, Easing.OutExpo);
                        logo?.ScaleTo(1, 800, Easing.OutExpo);
                    }, buttonArea.Alpha * 150);
                    break;

                case ButtonSystemState.TopLevel:
                case ButtonSystemState.Play:
                    switch (lastState)
                    {
                        case ButtonSystemState.TopLevel: // coming from toplevel to play
                            break;

                        case ButtonSystemState.Initial:
                            logo.ClearTransforms(targetMember: nameof(Position));

                            bool impact = logo.Scale.X > 0.6f;

                            logo.ScaleTo(0.5f, 200, Easing.In);

                            logoTrackingContainer.StartTracking(logo, 200, Easing.In);

                            logoDelayedAction?.Cancel();
                            logoDelayedAction = Scheduler.AddDelayed(() =>
                            {
                                if (impact)
                                    logo?.Impact();

                                game?.Toolbar.Show();
                            }, 200);
                            break;

                        default:
                            logo.ClearTransforms(targetMember: nameof(Position));
                            logoTrackingContainer.StartTracking(logo, 0, Easing.In);
                            logo.ScaleTo(0.5f, 200, Easing.OutQuint);
                            break;
                    }

                    break;

                case ButtonSystemState.EnteringMode:
                    logoTrackingContainer.StartTracking(logo, lastState == ButtonSystemState.Initial ? MainMenu.FADE_OUT_DURATION : 0, Easing.InSine);
                    break;
            }
        }
    }

    public enum ButtonSystemState
    {
        Exit,
        Initial,
        TopLevel,
        Play,
        EnteringMode,
    }
}
