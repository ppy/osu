// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    public class ButtonSystem : Container, IStateful<ButtonSystemState>, IKeyBindingHandler<GlobalAction>
    {
        public event Action<ButtonSystemState> StateChanged;

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
        }

        private readonly Drawable iconFacade;
        private readonly ButtonArea buttonArea;

        private readonly Button backButton;

        private readonly List<Button> buttonsTopLevel = new List<Button>();
        private readonly List<Button> buttonsPlay = new List<Button>();

        private SampleChannel sampleBack;

        public ButtonSystem()
        {
            RelativeSizeAxes = Axes.Both;

            Child = buttonArea = new ButtonArea();

            buttonArea.AddRange(new[]
            {
                new Button(@"settings", string.Empty, FontAwesome.fa_gear, new Color4(85, 85, 85, 255), () => OnSettings?.Invoke(), -WEDGE_WIDTH, Key.O),
                backButton = new Button(@"back", @"button-back-select", FontAwesome.fa_osu_left_o, new Color4(51, 58, 94, 255), () => State = ButtonSystemState.TopLevel, -WEDGE_WIDTH)
                {
                    VisibleState = ButtonSystemState.Play,
                },
                iconFacade = new Container //need a container to make the osu! icon flow properly.
                {
                    Size = new Vector2(0, ButtonArea.BUTTON_AREA_HEIGHT)
                }
            });

            buttonArea.Flow.CentreTarget = iconFacade;

            buttonsPlay.Add(new Button(@"solo", @"button-solo-select", FontAwesome.fa_user, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(), WEDGE_WIDTH, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"button-generic-select", FontAwesome.fa_users, new Color4(94, 63, 186, 255), () => OnMulti?.Invoke(), 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"button-generic-select", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), () => OnChart?.Invoke()));
            buttonsPlay.ForEach(b => b.VisibleState = ButtonSystemState.Play);

            buttonsTopLevel.Add(new Button(@"play", @"button-play-select", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), () => State = ButtonSystemState.Play, WEDGE_WIDTH, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"button-generic-select", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(), 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", @"button-direct-select", FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), () => OnDirect?.Invoke(), 0, Key.D));
            buttonsTopLevel.Add(new Button(@"exit", string.Empty, FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), () => OnExit?.Invoke(), 0, Key.Q));

            buttonArea.AddRange(buttonsPlay);
            buttonArea.AddRange(buttonsTopLevel);
        }

        private OsuGame game;

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuGame game)
        {
            this.game = game;
            sampleBack = audio.Sample.Get(@"Menu/button-back-select");
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
            get { return state; }

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
                            logoTracking = false;

                            game?.Toolbar.Hide();

                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.RelativePositionAxes = Axes.Both;

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
                            logo.RelativePositionAxes = Axes.None;

                            bool impact = logo.Scale.X > 0.6f;

                            if (lastState == ButtonSystemState.Initial)
                                logo.ScaleTo(0.5f, 200, Easing.In);

                            logo.MoveTo(logoTrackingPosition, lastState == ButtonSystemState.EnteringMode ? 0 : 200, Easing.In);

                            logoDelayedAction?.Cancel();
                            logoDelayedAction = Scheduler.AddDelayed(() =>
                            {
                                logoTracking = true;

                                if (impact)
                                    logo.Impact();

                                game?.Toolbar.Show();
                            }, 200);
                            break;
                        default:
                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.RelativePositionAxes = Axes.None;
                            logoTracking = true;
                            logo.ScaleTo(0.5f, 200, Easing.OutQuint);
                            break;
                    }

                    break;
                case ButtonSystemState.EnteringMode:
                    logoTracking = true;
                    break;
            }
        }

        private Vector2 logoTrackingPosition => logo.Parent.ToLocalSpace(iconFacade.ScreenSpaceDrawQuad.Centre);

        private bool logoTracking;

        protected override void Update()
        {
            //if (OsuGame.IdleTime > 6000 && State != MenuState.Exit)
            //    State = MenuState.Initial;

            base.Update();

            if (logo != null)
            {
                if (logoTracking && logo.RelativePositionAxes == Axes.None && iconFacade.IsLoaded)
                    logo.Position = logoTrackingPosition;

                iconFacade.Width = logo.SizeForFlow * 0.5f;
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
