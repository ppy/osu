﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// Button designed specifically for the osu!next main menu.
    /// In order to correctly flow, we have to use a negative margin on the parent container (due to the parallelogram shape).
    /// </summary>
    public class Button : BeatSyncedContainer, IStateful<ButtonState>
    {
        public event Action<ButtonState> StateChanged;

        private readonly Container iconText;
        private readonly Container box;
        private readonly Box boxHoverLayer;
        private readonly SpriteIcon icon;
        private readonly string sampleName;
        private readonly Action clickAction;
        private readonly Key triggerKey;
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => box.ReceiveMouseInputAt(screenSpacePos);

        public Button(string text, string sampleName, FontAwesome symbol, Color4 colour, Action clickAction = null, float extraWidth = 0, Key triggerKey = Key.Unknown)
        {
            this.sampleName = sampleName;
            this.clickAction = clickAction;
            this.triggerKey = triggerKey;

            AutoSizeAxes = Axes.Both;
            Alpha = 0;

            Vector2 boxSize = new Vector2(ButtonSystem.BUTTON_WIDTH + Math.Abs(extraWidth), ButtonSystem.BUTTON_AREA_HEIGHT);

            Children = new Drawable[]
            {
                box = new Container
                {
                    Masking = true,
                    MaskingSmoothness = 2,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.2f),
                        Roundness = 5,
                        Radius = 8,
                    },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0, 1),
                    Size = boxSize,
                    Shear = new Vector2(ButtonSystem.WEDGE_WIDTH / boxSize.Y, 0),
                    Children = new[]
                    {
                        new Box
                        {
                            EdgeSmoothness = new Vector2(1.5f, 0),
                            RelativeSizeAxes = Axes.Both,
                            Colour = colour,
                        },
                        boxHoverLayer = new Box
                        {
                            EdgeSmoothness = new Vector2(1.5f, 0),
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingMode.Additive,
                            Colour = Color4.White,
                            Alpha = 0,
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
                        icon = new SpriteIcon
                        {
                            Shadow = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(30),
                            Position = new Vector2(0, 0),
                            Icon = symbol
                        },
                        new OsuSpriteText
                        {
                            Shadow = true,
                            AllowMultiline = false,
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

        private bool rightward;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (!IsHovered) return;

            double duration = timingPoint.BeatLength / 2;

            icon.RotateTo(rightward ? 10 : -10, duration * 2, Easing.InOutSine);

            icon.Animate(
                i => i.MoveToY(-10, duration, Easing.Out),
                i => i.ScaleTo(1, duration, Easing.Out)
            ).Then(
                i => i.MoveToY(0, duration, Easing.In),
                i => i.ScaleTo(new Vector2(1, 0.9f), duration, Easing.In)
            );

            rightward = !rightward;
        }

        protected override bool OnHover(InputState state)
        {
            if (State != ButtonState.Expanded) return true;

            sampleHover?.Play();

            box.ScaleTo(new Vector2(1.5f, 1), 500, Easing.OutElastic);

            double duration = TimeUntilNextBeat;

            icon.ClearTransforms();
            icon.RotateTo(rightward ? -10 : 10, duration, Easing.InOutSine);
            icon.ScaleTo(new Vector2(1, 0.9f), duration, Easing.Out);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            icon.ClearTransforms();
            icon.RotateTo(0, 500, Easing.Out);
            icon.MoveTo(Vector2.Zero, 500, Easing.Out);
            icon.ScaleTo(Vector2.One, 200, Easing.Out);

            if (State == ButtonState.Expanded)
                box.ScaleTo(new Vector2(1, 1), 500, Easing.OutElastic);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"Menu/button-hover");
            if (!string.IsNullOrEmpty(sampleName))
                sampleClick = audio.Sample.Get($@"Menu/{sampleName}");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            boxHoverLayer.FadeTo(0.1f, 1000, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            boxHoverLayer.FadeTo(0, 1000, Easing.OutQuint);
            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            trigger();
            return true;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat || state.Keyboard.ControlPressed || state.Keyboard.ShiftPressed || state.Keyboard.AltPressed)
                return false;

            if (triggerKey == args.Key && triggerKey != Key.Unknown)
            {
                trigger();
                return true;
            }

            return false;
        }

        private void trigger()
        {
            sampleClick?.Play();

            clickAction?.Invoke();

            boxHoverLayer.ClearTransforms();
            boxHoverLayer.Alpha = 0.9f;
            boxHoverLayer.FadeOut(800, Easing.OutExpo);
        }

        public override bool HandleKeyboardInput => state != ButtonState.Exploded;
        public override bool HandleMouseInput => state != ButtonState.Exploded && box.Scale.X >= 0.8f;

        protected override void Update()
        {
            iconText.Alpha = MathHelper.Clamp((box.Scale.X - 0.5f) / 0.3f, 0, 1);
            base.Update();
        }

        public int ContractStyle;

        private ButtonState state;

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
                                box.ScaleTo(new Vector2(0, 1), 500, Easing.OutExpo);
                                this.FadeOut(500);
                                break;
                            case 1:
                                box.ScaleTo(new Vector2(0, 1), 400, Easing.InSine);
                                this.FadeOut(800);
                                break;
                        }
                        break;
                    case ButtonState.Expanded:
                        const int expand_duration = 500;
                        box.ScaleTo(new Vector2(1, 1), expand_duration, Easing.OutExpo);
                        this.FadeIn(expand_duration / 6f);
                        break;
                    case ButtonState.Exploded:
                        const int explode_duration = 200;
                        box.ScaleTo(new Vector2(2, 1), explode_duration, Easing.OutExpo);
                        this.FadeOut(explode_duration / 4f * 3);
                        break;
                }

                StateChanged?.Invoke(State);
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
