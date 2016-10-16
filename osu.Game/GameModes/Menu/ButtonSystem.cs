//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework;

namespace osu.Game.GameModes.Menu
{
    public partial class ButtonSystem : Container, IStateful<MenuState>
    {
        public Action OnEdit;
        public Action OnExit;
        public Action OnDirect;
        public Action OnSolo;
        public Action OnSettings;
        public Action OnMulti;
        public Action OnChart;
        public Action OnTest;

        private FlowContainerWithOrigin buttonFlow;

        //todo: make these non-internal somehow.
        internal const float button_area_height = 100;
        internal const float button_width = 140f;
        internal const float wedge_width = 20;

        public const int EXIT_DELAY = 3000;

        private OsuLogo osuLogo;
        private Drawable iconFacade;
        private Container buttonArea;
        private Box buttonAreaBackground;

        private Button backButton;
        private Button settingsButton;

        List<Button> buttonsTopLevel = new List<Button>();
        List<Button> buttonsPlay = new List<Button>();

        public ButtonSystem()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Children = new Drawable[]
            {
                buttonArea = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, button_area_height),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        buttonAreaBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(2, 1),
                            Colour = new Color4(50, 50, 50, 255),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        buttonFlow = new FlowContainerWithOrigin
                        {
                            Direction = FlowDirection.HorizontalOnly,
                            Anchor = Anchor.Centre,
                            Spacing = new Vector2(-wedge_width, 0),
                            Children = new Drawable[]
                            {
                                settingsButton = new Button(@"settings", @"options", FontAwesome.gear, new Color4(85, 85, 85, 255), OnSettings, -wedge_width, Key.O),
                                backButton = new Button(@"back", @"back", FontAwesome.fa_osu_left_o, new Color4(51, 58, 94, 255), onBack, -wedge_width, Key.Escape),
                                iconFacade = new Container //need a container to make the osu! icon flow properly.
								{
                                    Size = new Vector2(0, button_area_height)
                                }
                            },
                            CentreTarget = iconFacade
                        }
                    }
                },
                osuLogo = new OsuLogo
                {
                    Action = onOsuLogo,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                }
            };

            buttonFlow.Position = new Vector2(wedge_width * 2 - (button_width + osuLogo.SizeForFlow / 4), 0);

            buttonsPlay.Add(new Button(@"solo", @"freeplay", FontAwesome.user, new Color4(102, 68, 204, 255), OnSolo, wedge_width, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"multiplayer", FontAwesome.users, new Color4(94, 63, 186, 255), OnMulti, 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"charts", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), OnChart));

            buttonsTopLevel.Add(new Button(@"play", @"play", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), onPlay, wedge_width, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"edit", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), OnEdit, 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", @"direct", FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), OnDirect, 0, Key.D));
            buttonsTopLevel.Add(new Button(@"exit", @"exit", FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), onExit, 0, Key.Q));

            buttonFlow.Add(buttonsPlay);
            buttonFlow.Add(buttonsTopLevel);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Space:
                    osuLogo.TriggerClick(state);
                    return true;
                case Key.Escape:
                    if (State == MenuState.Initial)
                        return false;

                    State = MenuState.Initial;
                    return true;
            }

            return true;
        }

        private void onPlay()
        {
            State = MenuState.Play;
        }

        private void onExit()
        {
            State = MenuState.Exit;
            OnExit?.Invoke();
        }

        private void onBack()
        {
            State = MenuState.TopLevel;
        }

        private void onOsuLogo()
        {
            switch (state)
            {
                case MenuState.Initial:
                    //Game.Audio.PlaySamplePositional(@"menuhit");
                    State = MenuState.TopLevel;
                    return;
                case MenuState.TopLevel:
                    buttonsTopLevel.First().TriggerMouseDown();
                    return;
                case MenuState.Play:
                    buttonsPlay.First().TriggerMouseDown();
                    return;
            }
        }

        MenuState state;

        public override bool HandleInput => state != MenuState.Exit;

        public MenuState State
        {
            get
            {
                return state;
            }
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

                switch (state)
                {
                    case MenuState.Initial:
                        buttonAreaBackground.ScaleTo(Vector2.One, 500, EasingTypes.Out);
                        buttonArea.FadeOut(500);

                        osuLogo.Delay(150);
                        osuLogo.MoveTo(Vector2.Zero, 800, EasingTypes.OutExpo);
                        osuLogo.ScaleTo(1, 800, EasingTypes.OutExpo);

                        foreach (Button b in buttonsTopLevel)
                            b.State = ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = ButtonState.Contracted;
                        break;
                    case MenuState.TopLevel:
                        buttonAreaBackground.ScaleTo(Vector2.One, 200, EasingTypes.Out);

                        osuLogo.MoveTo(buttonFlow.Position, 200, EasingTypes.In);
                        osuLogo.ScaleTo(0.5f, 200, EasingTypes.In);

                        buttonArea.FadeIn(300);

                        if (lastState == MenuState.Initial)
                            //todo: this propagates to invisible children and causes delays later down the track (on first MenuState.Play)
                            buttonArea.Delay(150, true);

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
                        buttonAreaBackground.ScaleTo(new Vector2(2, 0), 300, EasingTypes.InSine);

                        buttonsTopLevel.ForEach(b => b.ContractStyle = 1);
                        buttonsPlay.ForEach(b => b.ContractStyle = 1);
                        backButton.ContractStyle = 1;
                        settingsButton.ContractStyle = 1;

                        foreach (Button b in buttonsTopLevel)
                            b.State = ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = ButtonState.Contracted;
                        break;
                    case MenuState.Exit:
                        buttonArea.FadeOut(200);

                        foreach (Button b in buttonsTopLevel)
                            b.State = ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = ButtonState.Contracted;

                        osuLogo.Delay(150);

                        osuLogo.ScaleTo(1f, EXIT_DELAY * 1.5f);
                        osuLogo.RotateTo(20, EXIT_DELAY * 1.5f);
                        osuLogo.FadeOut(EXIT_DELAY);
                        break;
                }

                backButton.State = state == MenuState.Play ? ButtonState.Expanded : ButtonState.Contracted;
                settingsButton.State = state == MenuState.TopLevel ? ButtonState.Expanded : ButtonState.Contracted;

                if (lastState == MenuState.Initial)
                    buttonArea.DelayReset();
            }
        }

        protected override void Update()
        {
            //if (OsuGame.IdleTime > 6000 && State != MenuState.Exit)
            //    State = MenuState.Initial;

            iconFacade.Width = osuLogo.SizeForFlow * 0.5f;
            base.Update();
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
