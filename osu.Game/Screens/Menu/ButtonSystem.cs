// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Menu
{
    public class ButtonSystem : Container, IStateful<ButtonSystemState>, IKeyBindingHandler<GlobalAction>
    {
        public event Action<ButtonSystemState> StateChanged;

        private readonly IBindable<bool> isIdle = new BindableBool();

        public Action OnEdit;
        public Action OnExit;
        public Action OnDirect;
        public Action OnSolo;
        public Action OnSettings;
        public Action OnMulti;
        public Action OnChart;

        public const float BUTTON_WIDTH = 140f;
        public const float WEDGE_WIDTH = 20;

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

        private readonly Button backButton;

        private readonly List<Button> buttonsTopLevel = new List<Button>();
        private readonly List<Button> buttonsPlay = new List<Button>();

        private SampleChannel sampleBack;

        private readonly LogoTrackingContainer logoTrackingContainer;

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
                new Button(@"settings", string.Empty, FontAwesome.Solid.Cog, new Color4(85, 85, 85, 255), () => OnSettings?.Invoke(), -WEDGE_WIDTH, Key.O),
                backButton = new Button(@"back", @"button-back-select", OsuIcon.LeftCircle, new Color4(51, 58, 94, 255), () => State = ButtonSystemState.TopLevel, -WEDGE_WIDTH)
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
        private NotificationOverlay notifications { get; set; }

        [Resolved(CanBeNull = true)]
        private LoginOverlay loginOverlay { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, IdleTracker idleTracker, GameHost host)
        {
            buttonsPlay.Add(new Button(@"solo", @"button-solo-select", FontAwesome.Solid.User, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(), WEDGE_WIDTH, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"button-generic-select", FontAwesome.Solid.Users, new Color4(94, 63, 186, 255), onMulti, 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"button-generic-select", OsuIcon.Charts, new Color4(80, 53, 160, 255), () => OnChart?.Invoke()));
            buttonsPlay.ForEach(b => b.VisibleState = ButtonSystemState.Play);

            buttonsTopLevel.Add(new Button(@"play", @"button-play-select", OsuIcon.Logo, new Color4(102, 68, 204, 255), () => State = ButtonSystemState.Play, WEDGE_WIDTH, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"button-generic-select", OsuIcon.EditCircle, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(), 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", @"button-direct-select", OsuIcon.ChevronDownCircle, new Color4(165, 204, 0, 255), () => OnDirect?.Invoke(), 0, Key.D));

            if (host.CanExit)
                buttonsTopLevel.Add(new Button(@"exit", string.Empty, OsuIcon.CrossCircle, new Color4(238, 51, 153, 255), () => OnExit?.Invoke(), 0, Key.Q));

            buttonArea.AddRange(buttonsPlay);
            buttonArea.AddRange(buttonsTopLevel);

            buttonArea.ForEach(b =>
            {
                if (b is Button)
                {
                    b.Origin = Anchor.CentreLeft;
                    b.Anchor = Anchor.CentreLeft;
                }
            });

            isIdle.ValueChanged += idle => updateIdleState(idle.NewValue);

            if (idleTracker != null) isIdle.BindTo(idleTracker.IsIdle);

            sampleBack = audio.Samples.Get(@"Menu/button-back-select");
        }

        private void onMulti()
        {
            if (!api.IsLoggedIn)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "You gotta be logged in to multi 'yo!",
                    Icon = FontAwesome.Solid.Globe,
                    Activated = () =>
                    {
                        loginOverlay?.Show();
                        return true;
                    }
                });

                return;
            }

            OnMulti?.Invoke();
        }

        private void updateIdleState(bool isIdle)
        {
            if (isIdle && State != ButtonSystemState.Exit && State != ButtonSystemState.EnteringMode)
                State = ButtonSystemState.Initial;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (State == ButtonSystemState.Initial)
            {
                if (buttonsTopLevel.Any(b => e.Key == b.TriggerKey))
                {
                    logo?.Click();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    return goBack();

                case GlobalAction.Select:
                    logo?.Click();
                    return true;

                default:
                    return false;
            }
        }

        public bool OnReleased(GlobalAction action) => false;

        private bool goBack()
        {
            switch (State)
            {
                case ButtonSystemState.TopLevel:
                    State = ButtonSystemState.Initial;
                    sampleBack?.Play();
                    return true;

                case ButtonSystemState.Play:
                    backButton.Click();
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
                    return true;

                case ButtonSystemState.Initial:
                    State = ButtonSystemState.TopLevel;
                    return true;

                case ButtonSystemState.TopLevel:
                    buttonsTopLevel.First().Click();
                    return false;

                case ButtonSystemState.Play:
                    buttonsPlay.First().Click();
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

                if (game != null)
                    game.OverlayActivationMode.Value = state == ButtonSystemState.Exit ? OverlayActivation.Disabled : OverlayActivation.All;

                updateLogoState(lastState);

                Logger.Log($"{nameof(ButtonSystem)}'s state changed from {lastState} to {state}");

                using (buttonArea.BeginDelayedSequence(lastState == ButtonSystemState.Initial ? 150 : 0, true))
                {
                    buttonArea.ButtonSystemState = state;

                    foreach (var b in buttonArea.Children.OfType<Button>())
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

                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.MoveTo(new Vector2(0.5f), 800, Easing.OutExpo);
                            logo.ScaleTo(1, 800, Easing.OutExpo);
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

                            if (lastState == ButtonSystemState.Initial)
                                logo.ScaleTo(0.5f, 200, Easing.In);

                            logoTrackingContainer.StartTracking(logo, lastState == ButtonSystemState.EnteringMode ? 0 : 200, Easing.In);

                            logoDelayedAction?.Cancel();
                            logoDelayedAction = Scheduler.AddDelayed(() =>
                            {
                                if (impact)
                                    logo.Impact();

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
