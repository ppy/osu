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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    public class ButtonSystem : Container, IStateful<MenuState>, IKeyBindingHandler<GlobalAction>
    {
        public event Action<MenuState> StateChanged;

        public Action OnEdit;
        public Action OnExit;
        public Action OnDirect;
        public Action OnSolo;
        public Action OnSettings;
        public Action OnMulti;
        public Action OnChart;
        public Action OnTest;

        private readonly FlowContainerWithOrigin buttonFlow;

        //todo: make these non-internal somehow.
        public const float BUTTON_AREA_HEIGHT = 100;

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
                buttonFlow.Position = new Vector2(WEDGE_WIDTH * 2 - (BUTTON_WIDTH + this.logo.SizeForFlow / 4), 0);

                updateLogoState();
            }
        }

        private readonly Drawable iconFacade;
        private readonly Container buttonArea;
        private readonly Box buttonAreaBackground;

        private readonly Button backButton;
        private readonly Button settingsButton;

        private readonly List<Button> buttonsTopLevel = new List<Button>();
        private readonly List<Button> buttonsPlay = new List<Button>();

        private SampleChannel sampleBack;

        public ButtonSystem()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                buttonArea = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, BUTTON_AREA_HEIGHT),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        buttonAreaBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(2, 1),
                            Colour = OsuColour.Gray(50),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        buttonFlow = new FlowContainerWithOrigin
                        {
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(-WEDGE_WIDTH, 0),
                            Anchor = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                settingsButton = new Button(@"settings", string.Empty, FontAwesome.fa_gear, new Color4(85, 85, 85, 255), () => OnSettings?.Invoke(), -WEDGE_WIDTH, Key.O),
                                backButton = new Button(@"back", string.Empty, FontAwesome.fa_osu_left_o, new Color4(51, 58, 94, 255), onBack, -WEDGE_WIDTH),
                                iconFacade = new Container //need a container to make the osu! icon flow properly.
                                {
                                    Size = new Vector2(0, BUTTON_AREA_HEIGHT)
                                }
                            },
                            CentreTarget = iconFacade
                        }
                    }
                },
            };

            buttonsPlay.Add(new Button(@"solo", @"button-solo-select", FontAwesome.fa_user, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(), WEDGE_WIDTH, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"button-generic-select", FontAwesome.fa_users, new Color4(94, 63, 186, 255), () => OnMulti?.Invoke(), 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"button-generic-select", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), () => OnChart?.Invoke()));

            buttonsTopLevel.Add(new Button(@"play", @"button-play-select", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), onPlay, WEDGE_WIDTH, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"button-generic-select", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(), 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", @"button-direct-select", FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), () => OnDirect?.Invoke(), 0, Key.D));
            buttonsTopLevel.Add(new Button(@"exit", string.Empty, FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), onExit, 0, Key.Q));

            buttonFlow.AddRange(buttonsPlay);
            buttonFlow.AddRange(buttonsTopLevel);
        }

        private OsuGame game;

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuGame game)
        {
            this.game = game;
            sampleBack = audio.Sample.Get(@"Menu/button-back-select");
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Space:
                    logo?.TriggerOnClick(state);
                    return true;
                case Key.Escape:
                    return goBack();
            }

            return false;
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    return goBack();
                default:
                    return false;
            }
        }

        private bool goBack()
        {
            switch (State)
            {
                case MenuState.TopLevel:
                    State = MenuState.Initial;
                    return true;
                case MenuState.Play:
                    backButton.TriggerOnClick();
                    return true;
                default:
                    return false;
            }
        }

        public bool OnReleased(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    return true;
                default:
                    return false;
            }
        }

        private void onPlay()
        {
            State = MenuState.Play;
        }

        private void onExit()
        {
            OnExit?.Invoke();
        }

        private void onBack()
        {
            sampleBack?.Play();
            State = MenuState.TopLevel;
        }

        private bool onOsuLogo()
        {
            switch (state)
            {
                default:
                    return true;
                case MenuState.Initial:
                    State = MenuState.TopLevel;
                    return true;
                case MenuState.TopLevel:
                    buttonsTopLevel.First().TriggerOnClick();
                    return false;
                case MenuState.Play:
                    buttonsPlay.First().TriggerOnClick();
                    return false;
            }
        }

        private MenuState state;

        public override bool HandleKeyboardInput => state != MenuState.Exit;
        public override bool HandleMouseInput => state != MenuState.Exit;

        public MenuState State
        {
            get { return state; }

            set
            {
                if (state == value) return;

                MenuState lastState = state;
                state = value;

                //todo: figure a more elegant way of doing this.
                buttonsTopLevel.ForEach(b => b.ContractStyle = 0);
                buttonsPlay.ForEach(b => b.ContractStyle = 0);
                backButton.ContractStyle = 0;
                settingsButton.ContractStyle = 0;

                updateLogoState(lastState);

                using (buttonArea.BeginDelayedSequence(lastState == MenuState.Initial ? 150 : 0, true))
                {
                    switch (state)
                    {
                        case MenuState.Exit:
                        case MenuState.Initial:
                            buttonAreaBackground.ScaleTo(Vector2.One, 500, Easing.Out);
                            buttonArea.FadeOut(300);

                            foreach (Button b in buttonsTopLevel)
                                b.State = ButtonState.Contracted;

                            foreach (Button b in buttonsPlay)
                                b.State = ButtonState.Contracted;

                            if (state != MenuState.Exit && lastState == MenuState.TopLevel)
                                sampleBack?.Play();
                            break;
                        case MenuState.TopLevel:
                            buttonAreaBackground.ScaleTo(Vector2.One, 200, Easing.Out);

                            buttonArea.FadeIn(300);

                            foreach (Button b in buttonsTopLevel)
                                b.State = ButtonState.Expanded;

                            foreach (Button b in buttonsPlay)
                                b.State = ButtonState.Contracted;
                            break;
                        case MenuState.Play:
                            foreach (Button b in buttonsTopLevel)
                                b.State = ButtonState.Exploded;

                            foreach (Button b in buttonsPlay)
                                b.State = ButtonState.Expanded;
                            break;
                        case MenuState.EnteringMode:
                            buttonAreaBackground.ScaleTo(new Vector2(2, 0), 300, Easing.InSine);

                            buttonsTopLevel.ForEach(b => b.ContractStyle = 1);
                            buttonsPlay.ForEach(b => b.ContractStyle = 1);
                            backButton.ContractStyle = 1;
                            settingsButton.ContractStyle = 1;

                            foreach (Button b in buttonsTopLevel)
                                b.State = ButtonState.Contracted;

                            foreach (Button b in buttonsPlay)
                                b.State = ButtonState.Contracted;
                            break;
                    }

                    backButton.State = state == MenuState.Play ? ButtonState.Expanded : ButtonState.Contracted;
                    settingsButton.State = state == MenuState.TopLevel ? ButtonState.Expanded : ButtonState.Contracted;
                }

                StateChanged?.Invoke(State);
            }
        }

        private ScheduledDelegate logoDelayedAction;

        private void updateLogoState(MenuState lastState = MenuState.Initial)
        {
            if (logo == null) return;

            switch (state)
            {
                case MenuState.Exit:
                case MenuState.Initial:
                    logoDelayedAction?.Cancel();
                    logoDelayedAction = Scheduler.AddDelayed(() =>
                        {
                            logoTracking = false;

                            if (game != null)
                            {
                                game.OverlayActivationMode.Value = state == MenuState.Exit ? OverlayActivation.Disabled : OverlayActivation.All;
                                game.Toolbar.Hide();
                            }

                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.RelativePositionAxes = Axes.Both;

                            logo.MoveTo(new Vector2(0.5f), 800, Easing.OutExpo);
                            logo.ScaleTo(1, 800, Easing.OutExpo);
                        }, buttonArea.Alpha * 150);
                    break;
                case MenuState.TopLevel:
                case MenuState.Play:
                    switch (lastState)
                    {
                        case MenuState.TopLevel: // coming from toplevel to play
                            break;
                        case MenuState.Initial:
                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.RelativePositionAxes = Axes.None;

                            bool impact = logo.Scale.X > 0.6f;

                            if (lastState == MenuState.Initial)
                                logo.ScaleTo(0.5f, 200, Easing.In);

                            logo.MoveTo(logoTrackingPosition, lastState == MenuState.EnteringMode ? 0 : 200, Easing.In);

                            logoDelayedAction?.Cancel();
                            logoDelayedAction = Scheduler.AddDelayed(() =>
                            {
                                logoTracking = true;

                                if (impact)
                                    logo.Impact();

                                if (game != null)
                                {
                                    game.OverlayActivationMode.Value = OverlayActivation.All;
                                    game.Toolbar.State = Visibility.Visible;
                                }
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
                case MenuState.EnteringMode:
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
                if (logoTracking)
                    logo.Position = logoTrackingPosition;

                iconFacade.Width = logo.SizeForFlow * 0.5f;
            }
        }
    }

    public enum MenuState
    {
        Initial,
        TopLevel,
        Play,
        EnteringMode,
        Exit,
    }
}
