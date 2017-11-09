// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Overlays.Toolbar;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio;

namespace osu.Game.Screens.Menu
{
    public class ButtonSystem : Container, IStateful<MenuState>
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

        private Toolbar toolbar;

        private readonly FlowContainerWithOrigin buttonFlow;

        //todo: make these non-internal somehow.
        internal const float BUTTON_AREA_HEIGHT = 100;

        internal const float BUTTON_WIDTH = 140f;
        internal const float WEDGE_WIDTH = 20;

        private OsuLogo logo;

        public void SetOsuLogo(OsuLogo logo)
        {
            this.logo = logo;

            if (this.logo != null)
            {
                this.logo.Action = onOsuLogo;

                // osuLogo.SizeForFlow relies on loading to be complete.
                buttonFlow.Position = new Vector2(WEDGE_WIDTH * 2 - (BUTTON_WIDTH + this.logo.SizeForFlow / 4), 0);
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

            buttonsPlay.Add(new Button(@"solo", @"select-6", FontAwesome.fa_user, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(), WEDGE_WIDTH, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"select-5", FontAwesome.fa_users, new Color4(94, 63, 186, 255), () => OnMulti?.Invoke(), 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"select-5", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), () => OnChart?.Invoke()));

            buttonsTopLevel.Add(new Button(@"play", @"select-1", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), onPlay, WEDGE_WIDTH, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"select-5", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(), 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", string.Empty, FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), () => OnDirect?.Invoke(), 0, Key.D));
            buttonsTopLevel.Add(new Button(@"exit", string.Empty, FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), onExit, 0, Key.Q));

            buttonFlow.AddRange(buttonsPlay);
            buttonFlow.AddRange(buttonsTopLevel);
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuGame game = null)
        {
            toolbar = game?.Toolbar;
            sampleBack = audio.Sample.Get(@"Menu/select-4");
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
                    switch (State)
                    {
                        case MenuState.TopLevel:
                            State = MenuState.Initial;
                            return true;
                        case MenuState.Play:
                            backButton.TriggerOnClick();
                            return true;
                    }


                    return false;
            }

            return false;
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

        private void onOsuLogo()
        {
            switch (state)
            {
                case MenuState.Initial:
                    State = MenuState.TopLevel;
                    return;
                case MenuState.TopLevel:
                    buttonsTopLevel.First().TriggerOnClick();
                    return;
                case MenuState.Play:
                    buttonsPlay.First().TriggerOnClick();
                    return;
            }
        }

        private MenuState state;

        public override bool HandleInput => state != MenuState.Exit;

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

                if (state == MenuState.TopLevel)
                    buttonArea.FinishTransforms(true);

                using (buttonArea.BeginDelayedSequence(lastState == MenuState.Initial ? 150 : 0, true))
                {
                    switch (state)
                    {
                        case MenuState.Exit:
                        case MenuState.Initial:
                            trackingPosition = false;

                            buttonAreaBackground.ScaleTo(Vector2.One, 500, Easing.Out);
                            buttonArea.FadeOut(300);

                            logo?.Delay(150)
                                .Schedule(() =>
                                {
                                    toolbar?.Hide();

                                    logo.ClearTransforms(targetMember: nameof(Position));
                                    logo.RelativePositionAxes = Axes.Both;

                                    logo.MoveTo(new Vector2(0.5f), 800, Easing.OutExpo);
                                    logo.ScaleTo(1, 800, Easing.OutExpo);
                                });

                            foreach (Button b in buttonsTopLevel)
                                b.State = ButtonState.Contracted;

                            foreach (Button b in buttonsPlay)
                                b.State = ButtonState.Contracted;

                            if (state != MenuState.Exit && lastState == MenuState.TopLevel)
                                sampleBack?.Play();
                            break;
                        case MenuState.TopLevel:
                            buttonAreaBackground.ScaleTo(Vector2.One, 200, Easing.Out);

                            logo.ClearTransforms(targetMember: nameof(Position));
                            logo.RelativePositionAxes = Axes.None;

                            trackingPosition = true;

                            switch (lastState)
                            {
                                case MenuState.Initial:
                                    logo.ScaleTo(0.5f, 200, Easing.In);

                                    trackingPosition = false;

                                    logo
                                        .MoveTo(iconTrackingPosition, lastState == MenuState.EnteringMode ? 0 : 200, Easing.In)
                                        .OnComplete(o =>
                                        {
                                            trackingPosition = true;

                                            o.Impact();
                                            toolbar?.Show();
                                        });
                                    break;
                                default:
                                    logo.ScaleTo(0.5f, 200, Easing.OutQuint);
                                    break;
                            }

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

                            trackingPosition = true;

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

        private Vector2 iconTrackingPosition => logo.Parent.ToLocalSpace(iconFacade.ScreenSpaceDrawQuad.Centre);

        private bool trackingPosition;

        protected override void Update()
        {
            //if (OsuGame.IdleTime > 6000 && State != MenuState.Exit)
            //    State = MenuState.Initial;

            base.Update();

            if (logo != null)
            {
                if (trackingPosition)
                    logo.Position = iconTrackingPosition;

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
