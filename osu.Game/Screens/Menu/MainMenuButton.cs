// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// Button designed specifically for the osu!next main menu.
    /// In order to correctly flow, we have to use a negative margin on the parent container (due to the parallelogram shape).
    /// </summary>
    public partial class MainMenuButton : BeatSyncedContainer, IStateful<ButtonState>
    {
        public const float BOUNCE_COMPRESSION = 0.9f;
        public const float HOVER_SCALE = 1.2f;
        public const float BOUNCE_ROTATION = 8;
        public event Action<ButtonState>? StateChanged;

        public readonly Key[] TriggerKeys;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        /// <summary>
        /// The menu state for which we are visible for (assuming only one).
        /// </summary>
        public ButtonSystemState VisibleState
        {
            set
            {
                VisibleStateMin = value;
                VisibleStateMax = value;
            }
        }

        public ButtonSystemState VisibleStateMin = ButtonSystemState.TopLevel;
        public ButtonSystemState VisibleStateMax = ButtonSystemState.TopLevel;

        public new MarginPadding Padding
        {
            get => Content.Padding;
            set => Content.Padding = value;
        }

        protected Vector2 BaseSize { get; init; } = new Vector2(ButtonSystem.BUTTON_WIDTH, ButtonArea.BUTTON_AREA_HEIGHT);

        private readonly Action<MainMenuButton, UIEvent>? clickAction;

        private readonly Container background;
        private readonly Drawable backgroundContent;
        private readonly Box boxHoverLayer;
        private readonly SpriteIcon icon;

        private Vector2 initialSize => BaseSize + Padding.Total;

        private readonly string sampleName;
        private Sample? sampleClick;
        private Sample? sampleHover;
        private SampleChannel? sampleChannel;

        public override bool IsPresent => base.IsPresent
                                          // Allow keyboard interaction based on state rather than waiting for delayed animations.
                                          || state == ButtonState.Expanded;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => background.ReceivePositionalInputAt(screenSpacePos);

        public MainMenuButton(LocalisableString text, string sampleName, IconUsage symbol, Color4 colour, Action<MainMenuButton, UIEvent>? clickAction = null, params Key[] triggerKeys)
        {
            this.sampleName = sampleName;
            this.clickAction = clickAction;
            TriggerKeys = triggerKeys;

            AutoSizeAxes = Axes.Both;
            Alpha = 0;

            AddRangeInternal(new Drawable[]
            {
                background = new Container
                {
                    // box needs to be always present to ensure the button is always sized correctly for flow
                    AlwaysPresent = true,
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
                    Children = new[]
                    {
                        backgroundContent = CreateBackground(colour).With(bg =>
                        {
                            bg.RelativeSizeAxes = Axes.Y;
                            bg.Anchor = Anchor.Centre;
                            bg.Origin = Anchor.Centre;
                        }),
                        boxHoverLayer = new Box
                        {
                            EdgeSmoothness = new Vector2(1.5f, 0),
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            Colour = Color4.White,
                            Depth = float.MinValue,
                            Alpha = 0,
                        },
                    }
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Shadow = true,
                            AllowMultiline = false,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding
                            {
                                Left = -3,
                                Bottom = 7,
                            },
                            Text = text
                        },
                        icon = new SpriteIcon
                        {
                            Shadow = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(32),
                            Position = new Vector2(0, 0),
                            Margin = new MarginPadding { Top = -4 },
                            Icon = symbol
                        }
                    }
                }
            });
        }

        protected virtual Drawable CreateBackground(Colour4 accentColour) => new Container
        {
            Child = new Box
            {
                EdgeSmoothness = new Vector2(1.5f, 0),
                RelativeSizeAxes = Axes.Both,
                Colour = accentColour,
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            background.Shear = new Vector2(ButtonSystem.WEDGE_WIDTH / initialSize.Y, 0);

            // for whatever reason, attempting to size the background "just in time" to cover the visible width
            // results in gaps when the width changes are quick (only visible when testing menu at 100% speed, not visible slowed down).
            // to ensure there's no missing backdrop, just use a ballpark that should be enough to always cover the width and then some.
            // note that while on a code inspections it would seem that `1.5 * initialSize.X` would be enough, elastic usings are used in this button
            // (which can exceed the [0;1] range during interpolation).
            backgroundContent.Width = 2 * initialSize.X;
            backgroundContent.Shear = -background.Shear;

            animateState();
            FinishTransforms(true);
        }

        private bool rightward;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (!IsHovered) return;

            double duration = timingPoint.BeatLength / 2;

            icon.RotateTo(rightward ? BOUNCE_ROTATION : -BOUNCE_ROTATION, duration * 2, Easing.InOutSine);

            icon.Animate(
                i => i.MoveToY(-10, duration, Easing.Out),
                i => i.ScaleTo(HOVER_SCALE, duration, Easing.Out)
            ).Then(
                i => i.MoveToY(0, duration, Easing.In),
                i => i.ScaleTo(new Vector2(HOVER_SCALE, HOVER_SCALE * BOUNCE_COMPRESSION), duration, Easing.In)
            );

            rightward = !rightward;
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (State != ButtonState.Expanded) return true;

            double duration = TimeUntilNextBeat;

            icon.ClearTransforms();
            icon.RotateTo(rightward ? -BOUNCE_ROTATION : BOUNCE_ROTATION, duration, Easing.InOutSine);
            icon.ScaleTo(new Vector2(HOVER_SCALE, HOVER_SCALE * BOUNCE_COMPRESSION), duration, Easing.Out);

            sampleHover?.Play();
            background.ResizeTo(Vector2.Multiply(initialSize, new Vector2(1.5f, 1)), 500, Easing.OutElastic);

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.ClearTransforms();
            icon.RotateTo(0, 500, Easing.Out);
            icon.MoveTo(Vector2.Zero, 500, Easing.Out);
            icon.ScaleTo(Vector2.One, 200, Easing.Out);

            if (State == ButtonState.Expanded)
                background.ResizeTo(initialSize, 500, Easing.OutElastic);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Samples.Get(@"Menu/button-hover");
            sampleClick = audio.Samples.Get(!string.IsNullOrEmpty(sampleName) ? $@"Menu/{sampleName}" : @"UI/button-select");
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            boxHoverLayer.FadeTo(0.1f, 1000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            boxHoverLayer.FadeTo(0, 1000, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            trigger(e);
            return true;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || e.ControlPressed || e.ShiftPressed || e.AltPressed || e.SuperPressed)
                return false;

            if (TriggerKeys.Contains(e.Key))
            {
                trigger(e);
                return true;
            }

            return false;
        }

        private void trigger(UIEvent e)
        {
            sampleChannel = sampleClick?.GetChannel();
            sampleChannel?.Play();

            clickAction?.Invoke(this, e);

            boxHoverLayer.ClearTransforms();
            boxHoverLayer.Alpha = 0.9f;
            boxHoverLayer.FadeOut(800, Easing.OutExpo);
        }

        public override bool HandleNonPositionalInput => state == ButtonState.Expanded;
        public override bool HandlePositionalInput => state != ButtonState.Exploded && background.Width / initialSize.X >= 0.8f;

        public void StopSamplePlayback() => sampleChannel?.Stop();

        protected override void Update()
        {
            content.Alpha = Math.Clamp((background.Width / initialSize.X - 0.5f) / 0.3f, 0, 1);
            base.Update();
        }

        public int ContractStyle;

        private ButtonState state;

        public ButtonState State
        {
            get => state;

            set
            {
                if (state == value)
                    return;

                state = value;

                animateState();

                StateChanged?.Invoke(State);
            }
        }

        private void animateState()
        {
            switch (state)
            {
                case ButtonState.Contracted:
                    switch (ContractStyle)
                    {
                        default:
                            background.ResizeTo(Vector2.Multiply(initialSize, new Vector2(0, 1)), 500, Easing.OutExpo);
                            this.FadeOut(500);
                            break;

                        case 1:
                            background.ResizeTo(Vector2.Multiply(initialSize, new Vector2(0, 1)), 400, Easing.InSine);
                            this.FadeOut(800);
                            break;
                    }

                    break;

                case ButtonState.Expanded:
                    const int expand_duration = 500;
                    background.ResizeTo(initialSize, expand_duration, Easing.OutExpo);
                    this.FadeIn(expand_duration / 6f);
                    break;

                case ButtonState.Exploded:
                    const int explode_duration = 200;
                    background.ResizeTo(Vector2.Multiply(initialSize, new Vector2(2, 1)), explode_duration, Easing.OutExpo);
                    this.FadeOut(explode_duration / 4f * 3);
                    break;
            }
        }

        private ButtonSystemState buttonSystemState;

        public ButtonSystemState ButtonSystemState
        {
            get => buttonSystemState;
            set
            {
                if (buttonSystemState == value)
                    return;

                buttonSystemState = value;
                UpdateState();
            }
        }

        protected virtual void UpdateState()
        {
            ContractStyle = 0;

            switch (ButtonSystemState)
            {
                case ButtonSystemState.Initial:
                    State = ButtonState.Contracted;
                    break;

                case ButtonSystemState.EnteringMode:
                    ContractStyle = 1;
                    State = ButtonState.Contracted;
                    break;

                default:
                    if (ButtonSystemState <= VisibleStateMax && ButtonSystemState >= VisibleStateMin)
                        State = ButtonState.Expanded;
                    else if (ButtonSystemState < VisibleStateMin)
                        State = ButtonState.Contracted;
                    else
                        State = ButtonState.Exploded;
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
}
