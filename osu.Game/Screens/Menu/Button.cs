//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// Button designed specifically for the osu!next main menu.
    /// In order to correctly flow, we have to use a negative margin on the parent container (due to the parallelogram shape).
    /// </summary>
    public class Button : Container, IStateful<ButtonState>
    {
        private Container iconText;
        private Container box;
        private Color4 colour;
        private TextAwesome icon;
        private string internalName;
        private readonly FontAwesome symbol;
        private Action clickAction;
        private readonly float extraWidth;
        private Key triggerKey;
        private string text;
        private AudioSample sampleClick;

        public override bool Contains(Vector2 screenSpacePos)
        {
            return box.Contains(screenSpacePos);
        }

        public Button(string text, string internalName, FontAwesome symbol, Color4 colour, Action clickAction = null, float extraWidth = 0, Key triggerKey = Key.Unknown)
        {
            this.internalName = internalName;
            this.symbol = symbol;
            this.colour = colour;
            this.clickAction = clickAction;
            this.extraWidth = extraWidth;
            this.triggerKey = triggerKey;
            this.text = text;

            AutoSizeAxes = Axes.Both;
            Alpha = 0;

            Vector2 boxSize = new Vector2(ButtonSystem.button_width + Math.Abs(extraWidth), ButtonSystem.button_area_height);

            Children = new Drawable[]
            {
                box = new Container
                {
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.2f),
                        Roundness = 5,
                        Radius = 8,
                    },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colour,
                    Scale = new Vector2(0, 1),
                    Size = boxSize,
                    Shear = new Vector2(ButtonSystem.wedge_width / boxSize.Y, 0),

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            EdgeSmoothness = new Vector2(2, 0),
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
                iconText = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Position = new Vector2(extraWidth / 2, 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        icon = new TextAwesome
                        {
                            Shadow = true,
                            Anchor = Anchor.Centre,
                            TextSize = 30,
                            Position = new Vector2(0, 0),
                            Icon = symbol
                        },
                        new SpriteText
                        {
                            Shadow = true,
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
            double startTime = Time.Current + offset;

            icon.RotateTo(10, offset, EasingTypes.InOutSine);
            icon.ScaleTo(new Vector2(1, 0.9f), offset, EasingTypes.Out);

            icon.Transforms.Add(new TransformRotation
            {
                StartValue = -10,
                EndValue = 10,
                StartTime = startTime,
                EndTime = startTime + duration * 2,
                Easing = EasingTypes.InOutSine,
                LoopCount = -1,
                LoopDelay = duration * 2
            });

            icon.Transforms.Add(new TransformPosition
            {
                StartValue = Vector2.Zero,
                EndValue = new Vector2(0, -10),
                StartTime = startTime,
                EndTime = startTime + duration,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = duration
            });

            icon.Transforms.Add(new TransformScale
            {
                StartValue = new Vector2(1, 0.9f),
                EndValue = Vector2.One,
                StartTime = startTime,
                EndTime = startTime + duration,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = duration
            });

            icon.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(0, -10),
                EndValue = Vector2.Zero,
                StartTime = startTime + duration,
                EndTime = startTime + duration * 2,
                Easing = EasingTypes.In,
                LoopCount = -1,
                LoopDelay = duration
            });

            icon.Transforms.Add(new TransformScale
            {
                StartValue = Vector2.One,
                EndValue = new Vector2(1, 0.9f),
                StartTime = startTime + duration,
                EndTime = startTime + duration * 2,
                Easing = EasingTypes.In,
                LoopCount = -1,
                LoopDelay = duration
            });

            icon.Transforms.Add(new TransformRotation
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

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get($@"Menu/menu-{internalName}-click");
            if (sampleClick == null)
                sampleClick = audio.Sample.Get(internalName.Contains(@"back") ? @"Menu/menuback" : @"Menu/menuhit");
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
            sampleClick.Play();

            clickAction?.Invoke();
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
    }

    public enum ButtonState
    {
        Contracted,
        Expanded,
        Exploded
    }
}
