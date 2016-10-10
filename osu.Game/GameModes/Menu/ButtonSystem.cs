//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework;

namespace osu.Game.GameModes.Menu
{
    public partial class ButtonSystem : Container
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

        const float button_area_height = 100;
        const float button_width = 140f;
        const float wedge_width = 20;

        public const int EXIT_DELAY = 3000;

        private OsuLogo osuLogo;
        private Drawable iconFacade;
        private Container buttonArea;
        private Box buttonAreaBackground;

        private Button backButton;
        private Button settingsButton;

        List<Button> buttonsTopLevel = new List<Button>();
        List<Button> buttonsPlay = new List<Button>();

        public enum MenuState
        {
            Initial,
            TopLevel,
            Play,
            EnteringMode,
            Exit,
        }

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
                            Anchor = Anchor.Centre,
                            Padding = new Vector2(-wedge_width, 0),
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
            if (args.Key == Key.Escape)
            {
                if (State == MenuState.Initial)
                    return false;

                State = MenuState.Initial;
                return true;
            }

            osuLogo.TriggerClick(state);
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
                            b.State = Button.ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Contracted;
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
                            b.State = Button.ButtonState.Expanded;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Contracted;
                        break;
                    case MenuState.Play:
                        foreach (Button b in buttonsTopLevel)
                            b.State = Button.ButtonState.Exploded;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Expanded;
                        break;
                    case MenuState.EnteringMode:
                        buttonAreaBackground.ScaleTo(new Vector2(2, 0), 300, EasingTypes.InSine);

                        buttonsTopLevel.ForEach(b => b.ContractStyle = 1);
                        buttonsPlay.ForEach(b => b.ContractStyle = 1);
                        backButton.ContractStyle = 1;
                        settingsButton.ContractStyle = 1;

                        foreach (Button b in buttonsTopLevel)
                            b.State = Button.ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Contracted;
                        break;
                    case MenuState.Exit:
                        buttonArea.FadeOut(200);

                        foreach (Button b in buttonsTopLevel)
                            b.State = Button.ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Contracted;

                        osuLogo.Delay(150);

                        osuLogo.ScaleTo(1f, EXIT_DELAY * 1.5f);
                        osuLogo.RotateTo(20, EXIT_DELAY * 1.5f);
                        osuLogo.FadeOut(EXIT_DELAY);
                        break;
                }

                backButton.State = state == MenuState.Play ? Button.ButtonState.Expanded : Button.ButtonState.Contracted;
                settingsButton.State = state == MenuState.TopLevel ? Button.ButtonState.Expanded : Button.ButtonState.Contracted;

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

        /// <summary>
        /// A flow container with an origin based on one of its contained drawables.
        /// </summary>
        private class FlowContainerWithOrigin : FlowContainer
        {
            /// <summary>
            /// A target drawable which this flowcontainer should be centered around.
            /// This target MUST be in this FlowContainer's *direct* children.
            /// </summary>
            internal Drawable CentreTarget;

            public override Anchor Origin => Anchor.Custom;

            public override Vector2 OriginPosition
            {
                get
                {
                    if (CentreTarget == null)
                        return base.OriginPosition;

                    return CentreTarget.Position + CentreTarget.Size / 2;
                }
            }

            public FlowContainerWithOrigin()
            {
                Direction = FlowDirection.HorizontalOnly;
            }
        }

        /// <summary>
        /// Button designed specifically for the osu!next main menu.
        /// In order to correctly flow, we have to use a negative margin on the parent container (due to the parallelogram shape).
        /// </summary>
        private class Button : AutoSizeContainer
        {
            private Container iconText;
            private WedgedBox box;
            private Color4 colour;
            private TextAwesome icon;
            private string internalName;
            private readonly FontAwesome symbol;
            private Action clickAction;
            private readonly float extraWidth;
            private Key triggerKey;
            private string text;

            public override Quad ScreenSpaceInputQuad => box.ScreenSpaceInputQuad;

            public Button(string text, string internalName, FontAwesome symbol, Color4 colour, Action clickAction = null, float extraWidth = 0, Key triggerKey = Key.Unknown)
            {
                this.internalName = internalName;
                this.symbol = symbol;
                this.colour = colour;
                this.clickAction = clickAction;
                this.extraWidth = extraWidth;
                this.triggerKey = triggerKey;
                this.text = text;
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);
                Alpha = 0;

                Children = new Drawable[]
                {
                    box = new WedgedBox(new Vector2(button_width + Math.Abs(extraWidth), button_area_height), wedge_width)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = colour,
                        Scale = new Vector2(0, 1)
                    },
                    iconText = new AutoSizeContainer
                    {
                        Position = new Vector2(extraWidth / 2, 0),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            icon = new TextAwesome
                            {
                                Anchor = Anchor.Centre,
                                TextSize = 30,
                                Position = new Vector2(0, 0),
                                Icon = symbol
                            },
                            new SpriteText
                            {
                                Direction = FlowDirection.HorizontalOnly,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                TextSize = 16,
                                Position = new Vector2(0, 35),
                                Text = text
                            }
                        }
                    }
                };
            }

            protected override bool OnHover(InputState state)
            {
                if (State != ButtonState.Expanded) return true;

                //if (OsuGame.Instance.IsActive)
                //    Game.Audio.PlaySamplePositional($@"menu-{internalName}-hover", @"menuclick");

                box.ScaleTo(new Vector2(1.5f, 1), 500, EasingTypes.OutElastic);

                int duration = 0; //(int)(Game.Audio.BeatLength / 2);
                if (duration == 0) duration = 250;

                icon.ClearTransformations();

                icon.ScaleTo(1, 500, EasingTypes.OutElasticHalf);

                double offset = 0; //(1 - Game.Audio.SyncBeatProgress) * duration;
                double startTime = Time + offset;

                icon.RotateTo(10, offset, EasingTypes.InOutSine);
                icon.ScaleTo(new Vector2(1, 0.9f), offset, EasingTypes.Out);

                icon.Transforms.Add(new TransformRotation(Clock)
                {
                    StartValue = -10,
                    EndValue = 10,
                    StartTime = startTime,
                    EndTime = startTime + duration * 2,
                    Easing = EasingTypes.InOutSine,
                    LoopCount = -1,
                    LoopDelay = duration * 2
                });

                icon.Transforms.Add(new TransformPosition(Clock)
                {
                    StartValue = Vector2.Zero,
                    EndValue = new Vector2(0, -10),
                    StartTime = startTime,
                    EndTime = startTime + duration,
                    Easing = EasingTypes.Out,
                    LoopCount = -1,
                    LoopDelay = duration
                });

                icon.Transforms.Add(new TransformScaleVector(Clock)
                {
                    StartValue = new Vector2(1, 0.9f),
                    EndValue = Vector2.One,
                    StartTime = startTime,
                    EndTime = startTime + duration,
                    Easing = EasingTypes.Out,
                    LoopCount = -1,
                    LoopDelay = duration
                });

                icon.Transforms.Add(new TransformPosition(Clock)
                {
                    StartValue = new Vector2(0, -10),
                    EndValue = Vector2.Zero,
                    StartTime = startTime + duration,
                    EndTime = startTime + duration * 2,
                    Easing = EasingTypes.In,
                    LoopCount = -1,
                    LoopDelay = duration
                });

                icon.Transforms.Add(new TransformScaleVector(Clock)
                {
                    StartValue = Vector2.One,
                    EndValue = new Vector2(1, 0.9f),
                    StartTime = startTime + duration,
                    EndTime = startTime + duration * 2,
                    Easing = EasingTypes.In,
                    LoopCount = -1,
                    LoopDelay = duration
                });

                icon.Transforms.Add(new TransformRotation(Clock)
                {
                    StartValue = 10,
                    EndValue = -10,
                    StartTime = startTime + duration * 2,
                    EndTime = startTime + duration * 4,
                    Easing = EasingTypes.InOutSine,
                    LoopCount = -1,
                    LoopDelay = duration * 2
                });

                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                icon.ClearTransformations();
                icon.RotateTo(0, 500, EasingTypes.Out);
                icon.MoveTo(Vector2.Zero, 500, EasingTypes.Out);
                icon.ScaleTo(0.7f, 500, EasingTypes.OutElasticHalf);
                icon.ScaleTo(Vector2.One, 200, EasingTypes.Out);

                if (State == ButtonState.Expanded)
                    box.ScaleTo(new Vector2(1, 1), 500, EasingTypes.OutElastic);
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                trigger();
                return true;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                base.OnKeyDown(state, args);

                if (triggerKey == args.Key && triggerKey != Key.Unknown)
                {
                    trigger();
                    return true;
                }

                return false;
            }

            private void trigger()
            {
                //Game.Audio.PlaySamplePositional($@"menu-{internalName}-click", internalName.Contains(@"back") ? @"menuback" : @"menuhit");

                clickAction?.Invoke();

                //box.FlashColour(ColourHelper.Lighten2(colour, 0.7f), 200);
            }

            public override bool HandleInput => state != ButtonState.Exploded && box.Scale.X >= 0.8f;

            protected override void Update()
            {
                iconText.Alpha = MathHelper.Clamp((box.Scale.X - 0.5f) / 0.3f, 0, 1);
                base.Update();
            }

            public int ContractStyle;

            ButtonState state;
            public ButtonState State
            {
                get { return state; }
                set
                {

                    if (state == value)
                        return;

                    state = value;

                    switch (state)
                    {
                        case ButtonState.Contracted:
                            switch (ContractStyle)
                            {
                                default:
                                    box.ScaleTo(new Vector2(0, 1), 500, EasingTypes.OutExpo);
                                    FadeOut(500);
                                    break;
                                case 1:
                                    box.ScaleTo(new Vector2(0, 1), 400, EasingTypes.InSine);
                                    FadeOut(800);
                                    break;
                            }
                            break;
                        case ButtonState.Expanded:
                            const int expand_duration = 500;
                            box.ScaleTo(new Vector2(1, 1), expand_duration, EasingTypes.OutExpo);
                            FadeIn(expand_duration / 6);
                            break;
                        case ButtonState.Exploded:
                            const int explode_duration = 200;
                            box.ScaleTo(new Vector2(2, 1), explode_duration, EasingTypes.OutExpo);
                            FadeOut(explode_duration / 4 * 3);
                            break;
                    }
                }
            }

            public enum ButtonState
            {
                Contracted,
                Expanded,
                Exploded
            }

            /// <summary>
            ///    ________
            ///   /       /
            ///  /       /
            /// /_______/
            /// </summary>
            class WedgedBox : Box
            {
                float wedgeWidth;

                public WedgedBox(Vector2 boxSize, float wedgeWidth)
                {
                    Size = boxSize;
                    this.wedgeWidth = wedgeWidth;
                }

                /// <summary>
                /// Custom DrawQuad used to create the slanted effect.
                /// </summary>
                protected override Quad DrawQuad
                {
                    get
                    {
                        Quad q = base.DrawQuad;

                        //Will become infinite if we don't limit its maximum size.
                        float wedge = Math.Min(q.Width, wedgeWidth / Scale.X);

                        q.TopLeft.X += wedge;
                        q.BottomRight.X -= wedge;

                        return q;
                    }
                }
            }
        }

        internal class MenuVisualisation : Drawable
        {
        }
    }
}
