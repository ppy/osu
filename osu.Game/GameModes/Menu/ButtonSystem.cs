//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT License - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.GameModes.Menu
{
    internal class ButtonSystem : OsuLargeComponent
    {
        private FlowContainerWithOrigin buttonFlow;

        const float button_area_height = 128;
        const float button_width = 180f;
        const float wedge_width = 25.6f;

        private OsuLogo osuLogo;
        private Drawable iconFacade;
        private Container buttonArea;

        private Button backButton;
        private Button settingsButton;

        List<Button> buttonsTopLevel = new List<Button>();
        List<Button> buttonsPlay = new List<Button>();

        public enum MenuState
        {
            Initial,
            Exit,
            TopLevel,
            Play,
        }

        public override void Load()
        {
            base.Load();

            osuLogo = new OsuLogo(onOsuLogo)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };

            Add(buttonArea = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                SizeMode = InheritMode.X,
                Size = new Vector2(1, button_area_height),
                Alpha = 0
            });

            Add(osuLogo);

            buttonArea.Add(new Box()
            {
                SizeMode = InheritMode.XY,
                Colour = new Color4(50, 50, 50, 255)
            });

            buttonArea.Add(buttonFlow = new FlowContainerWithOrigin()
            {
                Anchor = Anchor.Centre,
                Position = new Vector2(wedge_width * 2 - (button_width + osuLogo.SizeForFlow / 4), 0),
                Padding = new Vector2(-wedge_width, 0)
            });

            buttonFlow.Add(settingsButton = new Button(@"settings", @"options", FontAwesome.gear, new Color4(85, 85, 85, 255), onSettings, -wedge_width, Key.O));
            buttonFlow.Add(backButton = new Button(@"back", @"back", FontAwesome.fa_osu_left_o, new Color4(51, 58, 94, 255), onBack, -wedge_width, Key.Escape));

            //need a container to make the osu! icon flow properly.
            buttonFlow.Add(iconFacade = new Drawable() { Size = new Vector2(0, button_area_height) });

            buttonsPlay.Add((Button)buttonFlow.Add(new Button(@"solo", @"freeplay", FontAwesome.user, new Color4(102, 68, 204, 255), onSolo, wedge_width, Key.P)));
            buttonsPlay.Add((Button)buttonFlow.Add(new Button(@"multi", @"multiplayer", FontAwesome.users, new Color4(94, 63, 186, 255), onMulti, 0, Key.M)));
            buttonsPlay.Add((Button)buttonFlow.Add(new Button(@"chart", @"charts", FontAwesome.fa_osu_charts, new Color4(80, 53, 160, 255), onChart)));
            buttonsPlay.Add((Button)buttonFlow.Add(new Button(@"tests", @"tests", FontAwesome.terminal, new Color4(80, 53, 160, 255), onTest, 0, Key.T)));

            buttonsTopLevel.Add((Button)buttonFlow.Add(new Button(@"play", @"play", FontAwesome.fa_osu_logo, new Color4(102, 68, 204, 255), onPlay, wedge_width, Key.P)));
            buttonsTopLevel.Add((Button)buttonFlow.Add(new Button(@"osu!editor", @"edit", FontAwesome.fa_osu_edit_o, new Color4(238, 170, 0, 255), onEdit, 0, Key.E)));
            buttonsTopLevel.Add((Button)buttonFlow.Add(new Button(@"osu!direct", @"direct", FontAwesome.fa_osu_chevron_down_o, new Color4(165, 204, 0, 255), onDirect, 0, Key.D)));
            buttonsTopLevel.Add((Button)buttonFlow.Add(new Button(@"exit", @"exit", FontAwesome.fa_osu_cross_o, new Color4(238, 51, 153, 255), onExit, 0, Key.Q)));

            buttonFlow.CentreTarget = iconFacade;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            osuLogo.TriggerClick(state);
            return true;
        }

        private void onSettings()
        {
            //OsuGame.Options.LoginOnly = false;
            //OsuGame.Options.Expanded = true;
        }

        private void onPlay()
        {
            State = MenuState.Play;
        }

        private void onEdit()
        {
            //OsuGame.ChangeMode(OsuModes.SelectEdit);
        }

        private void onDirect()
        {
            //OsuGame.ChangeMode(OsuModes.OnlineSelection);
        }

        private void onExit()
        {
            //OsuGame.ChangeMode(OsuModes.Exit);
            State = MenuState.Exit;
        }

        private void onBack()
        {
            State = MenuState.TopLevel;
        }

        private void onSolo()
        {
            //OsuGame.ChangeMode(OsuModes.SelectPlay);
        }

        private void onMulti()
        {
            //OsuGame.ChangeMode(OsuModes.Lobby);
        }

        private void onChart()
        {
            //OsuGame.ChangeMode(OsuModes.Charts);
        }

        private void onTest()
        {

            //OsuGame.ChangeMode(OsuModes.FieldTest);
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

                switch (state)
                {
                    case MenuState.Initial:
                        backButton.State = Button.ButtonState.Contracted;

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
                        backButton.State = Button.ButtonState.Contracted;

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
                        backButton.State = Button.ButtonState.Expanded;

                        foreach (Button b in buttonsTopLevel)
                            b.State = Button.ButtonState.Exploded;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Expanded;
                        break;
                    case MenuState.Exit:
                        HandleInput = false;

                        buttonArea.FadeOut(200);

                        foreach (Button b in buttonsTopLevel)
                            b.State = Button.ButtonState.Contracted;

                        foreach (Button b in buttonsPlay)
                            b.State = Button.ButtonState.Contracted;

                        osuLogo.Delay(150);
                        osuLogo.ScaleTo(1f, 4000);
                        osuLogo.RotateTo(20, 4000);
                        osuLogo.FadeOut(4000);
                        break;
                }

                backButton.State = state >= MenuState.Play ? Button.ButtonState.Expanded : Button.ButtonState.Contracted;
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
        /// osu! logo and its attachments (pulsing, visualiser etc.)
        /// </summary>
        class OsuLogo : OsuComponent
        {
            private Sprite logo;
            private Container logoBounceContainer;
            private MenuVisualisation vis;
            private VoidDelegate clickAction;

            public float SizeForFlow => logo == null ? 0 : logo.ActualSize.X * logo.Scale * logoBounceContainer.Scale * 0.8f;

            public override void Load()
            {
                base.Load();
                logoBounceContainer = new AutoSizeContainer();

                logo = new Sprite(Game.Textures.Get(@"menu-osu"))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                Sprite ripple = new Sprite(Game.Textures.Get(@"menu-osu"))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0.4f
                };

                ripple.ScaleTo(1.1f, 500);
                ripple.FadeOut(500);
                ripple.Transformations.ForEach(t =>
                {
                    t.Loop = true;
                    t.LoopDelay = 300;
                });

                logoBounceContainer.Add(logo);
                logoBounceContainer.Add(ripple);
                logoBounceContainer.Add(vis = new MenuVisualisation()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = logo.Size,
                    Additive = true,
                    //Radius = logo.Size.X / 2 * 0.96f,
                    Alpha = 0.2f,
                });
                Add(logoBounceContainer);
            }

            public OsuLogo(VoidDelegate action)
            {
                clickAction = action;
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                logoBounceContainer.ScaleTo(1.1f, 1000, EasingTypes.Out);
                return true;
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                logoBounceContainer.ScaleTo(1.2f, 500, EasingTypes.OutElastic);
                return true;
            }

            protected override bool OnClick(InputState state)
            {
                clickAction?.Invoke();
                return true;
            }

            protected override bool OnHover(InputState state)
            {
                logoBounceContainer.ScaleTo(1.2f, 500, EasingTypes.OutElastic);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                logoBounceContainer.ScaleTo(1, 500, EasingTypes.OutElastic);
            }
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
        private class Button : OsuComponent
        {
            private Container iconText;
            private WedgedBox box;
            private Color4 colour;
            private TextAwesome icon;
            private string internalName;
            private readonly FontAwesome symbol;
            private VoidDelegate clickAction;
            private readonly float extraWidth;
            private Key triggerKey;
            private string text;

            public override Quad ScreenSpaceInputQuad => box.ScreenSpaceInputQuad;

            public Button(string text, string internalName, FontAwesome symbol, Color4 colour, VoidDelegate clickAction = null, float extraWidth = 0, Key triggerKey = Key.Unknown)
            {
                this.internalName = internalName;
                this.symbol = symbol;
                this.colour = colour;
                this.clickAction = clickAction;
                this.extraWidth = extraWidth;
                this.triggerKey = triggerKey;
                this.text = text;
            }

            public override void Load()
            {
                base.Load();
                Alpha = 0;

                Add(box = new WedgedBox(new Vector2(button_width + Math.Abs(extraWidth), button_area_height), wedge_width)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colour,
                    VectorScale = new Vector2(0, 1)
                });

                iconText = new AutoSizeContainer
                {
                    Position = new Vector2(extraWidth / 2, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                Add(iconText);

                icon = new TextAwesome(symbol, 40, Vector2.Zero)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = 0.7f,
                };
                iconText.Add(icon);

                SpriteText ft = new SpriteText()
                {
                    Direction = FlowDirection.HorizontalOnly,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(0, 25),
                    Text = text
                };
                iconText.Add(ft);
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

                icon.Transformations.Add(new Transformation(TransformationType.Rotation, -10, 10, startTime, startTime + duration * 2, EasingTypes.InOutSine) { Loop = true, LoopDelay = duration * 2 });
                icon.Transformations.Add(new Transformation(Vector2.Zero, new Vector2(0, -10), startTime, startTime + duration, EasingTypes.Out) { Loop = true, LoopDelay = duration });
                icon.Transformations.Add(new Transformation(TransformationType.VectorScale, new Vector2(1, 0.9f), Vector2.One, startTime, startTime + duration, EasingTypes.Out) { Loop = true, LoopDelay = duration });

                icon.Transformations.Add(new Transformation(new Vector2(0, -10), Vector2.Zero, startTime + duration, startTime + duration * 2, EasingTypes.In) { Loop = true, LoopDelay = duration });
                icon.Transformations.Add(new Transformation(TransformationType.VectorScale, Vector2.One, new Vector2(1, 0.9f), startTime + duration, startTime + duration * 2, EasingTypes.In) { Loop = true, LoopDelay = duration });

                icon.Transformations.Add(new Transformation(TransformationType.Rotation, 10, -10, startTime + duration * 2, startTime + duration * 4, EasingTypes.InOutSine) { Loop = true, LoopDelay = duration * 2 });
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

            protected override void Update()
            {
                HandleInput = state != ButtonState.Exploded && box.VectorScale.X >= 0.8f;
                iconText.Alpha = MathHelper.Clamp((box.VectorScale.X - 0.5f) / 0.3f, 0, 1);
                base.Update();
            }

            ButtonState state;

            public ButtonState State
            {
                get { return state; }
                set
                {

                    if (state == value)
                        return;

                    ButtonState lastState = state;
                    state = value;

                    switch (state)
                    {
                        case ButtonState.Contracted:
                            const int contract_duration = 500;
                            box.ScaleTo(new Vector2(0, 1), contract_duration, EasingTypes.OutExpo);
                            FadeOut(contract_duration);
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
                        float wedge = Math.Min(q.Width, wedgeWidth / Scale / VectorScale.X);

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
