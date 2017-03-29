// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Overlays.Toolbar;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    public class ButtonSystem : Container, IStateful<MenuState>
    {
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

        public const int EXIT_DELAY = 3000;

        private readonly OsuLogo osuLogo;
        private readonly Drawable iconFacade;
        private readonly Container buttonArea;
        private readonly Box buttonAreaBackground;

        private readonly Button backButton;
        private readonly Button settingsButton;

        private readonly List<Button> buttonsTopLevel = new List<Button>();
        private readonly List<Button> buttonsPlay = new List<Button>();

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
                                settingsButton = new Button(@"settings", @"options", FontAwesome.fa_gear, new Color4(85, 85, 85, 255), () => OnSettings?.Invoke(), -WEDGE_WIDTH, Key.O),
                                backButton = new Button(@"back", @"back", FontAwesome.fa_osu_left_o, new Color4(51, 58, 94, 255), onBack, -WEDGE_WIDTH),
                                iconFacade = new Container //need a container to make the osu! icon flow properly.
                                {
                                    Size = new Vector2(0, BUTTON_AREA_HEIGHT)
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
                    Anchor = Anchor.Centre,
                }
            };

            buttonsPlay.Add(new Button(@"solo", @"freeplay", FontAwesome.fa_user, new Color4(102, 68, 204, 255), () => OnSolo?.Invoke(), WEDGE_WIDTH, Key.P));
            buttonsPlay.Add(new Button(@"multi", @"multiplayer", FontAwesome.fa_users, new Color4(94, 63, 186, 255), () => OnMulti?.Invoke(), 0, Key.M));
            buttonsPlay.Add(new Button(@"chart", @"charts", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), () => OnChart?.Invoke()));

            buttonsTopLevel.Add(new Button(@"play", @"play", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), onPlay, WEDGE_WIDTH, Key.P));
            buttonsTopLevel.Add(new Button(@"osu!editor", @"edit", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), () => OnEdit?.Invoke(), 0, Key.E));
            buttonsTopLevel.Add(new Button(@"osu!direct", @"direct", FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), () => OnDirect?.Invoke(), 0, Key.D));
            buttonsTopLevel.Add(new Button(@"exit", @"exit", FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), onExit, 0, Key.Q));

            buttonFlow.Add(buttonsPlay);
            buttonFlow.Add(buttonsTopLevel);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game = null)
        {
            toolbar = game?.Toolbar;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // osuLogo.SizeForFlow relies on loading to be complete.
            buttonFlow.Position = new Vector2(WEDGE_WIDTH * 2 - (BUTTON_WIDTH + osuLogo.SizeForFlow / 4), 0);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Space:
                    osuLogo.TriggerClick(state);
                    return true;
                case Key.Escape:
                    switch (State)
                    {
                        case MenuState.TopLevel:
                            State = MenuState.Initial;
                            return true;
                        case MenuState.Play:
                            backButton.TriggerClick();
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
                    buttonsTopLevel.First().TriggerClick();
                    return;
                case MenuState.Play:
                    buttonsPlay.First().TriggerClick();
                    return;
            }
        }

        private MenuState state;

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
                    case MenuState.Exit:
                    case MenuState.Initial:
                        toolbar?.Hide();

                        buttonAreaBackground.ScaleTo(Vector2.One, 500, EasingTypes.Out);
                        buttonArea.FadeOut(300);

                        osuLogo.Delay(150);
                        osuLogo.MoveTo(Vector2.Zero, 800, EasingTypes.OutExpo);
                        osuLogo.ScaleTo(1, 800, EasingTypes.OutExpo);

                        foreach (Button b in buttonsTopLevel)
                            b.State = ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = ButtonState.Contracted;

                        if (state == MenuState.Exit)
                        {
                            osuLogo.RotateTo(20, EXIT_DELAY * 1.5f);
                            osuLogo.FadeOut(EXIT_DELAY);
                        }
                        break;
                    case MenuState.TopLevel:
                        buttonArea.Flush(true);

                        buttonAreaBackground.ScaleTo(Vector2.One, 200, EasingTypes.Out);

                        osuLogo.MoveTo(buttonFlow.DrawPosition, 200, EasingTypes.In);
                        osuLogo.ScaleTo(0.5f, 200, EasingTypes.In);

                        buttonArea.FadeIn(300);

                        if (lastState == MenuState.Initial)
                            buttonArea.Delay(150, true);

                        Scheduler.AddDelayed(() => toolbar?.Show(), 150);

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

            osuLogo.Interactive = Alpha > 0.2f;

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
