// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
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
using osu.Framework.Lists;
using osu.Framework.Threading;

namespace osu.Game.Screens.Menu
{
    /// <summary>
    /// Button designed specifically for the osu!next main menu.
    /// In order to correctly flow, we have to use a negative margin on the parent container (due to the parallelogram shape).
    /// </summary>
    public class Button : BeatSyncedContainer, IStateful<ButtonState>
    {
        private readonly Container iconText;
        private readonly Container box;
        private readonly Box boxHoverLayer;
        private readonly TextAwesome icon;
        private readonly string internalName;
        private readonly Action clickAction;
        private readonly Key triggerKey;
        private SampleChannel sampleClick;

        public override bool Contains(Vector2 screenSpacePos) => box.Contains(screenSpacePos);

        public Button(string text, string internalName, FontAwesome symbol, Color4 colour, Action clickAction = null, float extraWidth = 0, Key triggerKey = Key.Unknown)
        {
            this.internalName = internalName;
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
                            BlendingMode = BlendingMode.Additive,
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
                        icon = new TextAwesome
                        {
                            Shadow = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 30,
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

        // true if icon going to jump from left to right
        private bool jumpSide;
        private ScheduledDelegate defaultAnimationDelegate;
        private double previousBeatTime;
        private bool isAnimated;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (Hovering)
            {
                // Beat length can be trimmed by the next timing point
                double beatTime = calculateBeatTime(timingPoint, getNextControlPoint(timingPoint));

                if(previousBeatTime != beatTime || !isAnimated)
                    restartAnimation(beatTime);

                // If animation will be restarted - we should know, on which side we've stopped
                // to start animation from the correct side
                jumpSide = !jumpSide;

                previousBeatTime = beatTime;
            }
        }

        private double calculateBeatTime(TimingControlPoint current, TimingControlPoint next)
        {
            double currentTime = Beatmap.Value.Track.CurrentTime;

            double difference = current.Time - currentTime;
            if (difference < current.BeatLength && difference > 0)
                return difference;

            if (current == next)
                return current.BeatLength;

            if (next.Time - current.Time < current.BeatLength)
                return next.Time - current.Time;

            difference = next.Time - currentTime;
            if (difference < current.BeatLength)
                return difference;

            return current.BeatLength;
        }

        protected override bool OnHover(InputState state)
        {
            if (State != ButtonState.Expanded) return true;

            box.ScaleTo(new Vector2(1.5f, 1), 500, EasingTypes.OutElastic);

            double offset = getNextBeatTimeLeft();

            icon.ClearTransforms();
            icon.MoveTo(Vector2.Zero, offset, EasingTypes.Out);
            icon.RotateTo(-10, offset, EasingTypes.InOutSine);
            icon.ScaleTo(new Vector2(1, 0.9f), offset, EasingTypes.Out);

            jumpSide = true;

            if (Beatmap.Value?.Track == null)
            {
                Delay(offset);
                defaultAnimationDelegate = Schedule(() => restartAnimation(500));
            }

            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            defaultAnimationDelegate?.Cancel();
            isAnimated = false;

            icon.ClearTransforms();
            icon.RotateTo(0, 500, EasingTypes.Out);
            icon.MoveTo(Vector2.Zero, 500, EasingTypes.Out);
            icon.ScaleTo(0.7f, 500, EasingTypes.OutElasticHalf);
            icon.ScaleTo(Vector2.One, 200, EasingTypes.Out);

            if (State == ButtonState.Expanded)
                box.ScaleTo(new Vector2(1, 1), 500, EasingTypes.OutElastic);
        }

        private double getNextBeatTimeLeft()
        {
            if (Beatmap.Value?.Track == null)
                return 200;

            double currentTime = Beatmap.Value.Track.CurrentTime;

            TimingControlPoint current = Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(currentTime);
            TimingControlPoint next = getNextControlPoint(current);

            // If the difference between the current time and the start time of the next timing point
            // less than beat time of the current timing point
            if (Beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(currentTime + current.BeatLength) == next && current != next)
                return next.Time - currentTime;

            return current.BeatLength - (currentTime - current.Time) % current.BeatLength;
        }

        private TimingControlPoint getNextControlPoint(TimingControlPoint current)
        {
            SortedList<TimingControlPoint> timingPoints = Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints;

            if (timingPoints[timingPoints.Count - 1] == current)
                return current;

            return timingPoints[timingPoints.IndexOf(current) + 1];
        }

        private void restartAnimation(double beatLength)
        {
            icon.ClearTransforms();

            double startTime = Time.Current;
            isAnimated = true;

            icon.Transforms.Add(new TransformRotation
            {
                StartValue = jumpSide ? -10 : 10,
                EndValue = jumpSide ? 10 : -10,
                StartTime = startTime,
                EndTime = startTime + beatLength,
                Easing = EasingTypes.InOutSine,
                LoopCount = -1,
                LoopDelay = beatLength
            });
            icon.Transforms.Add(new TransformPosition
            {
                StartValue = Vector2.Zero,
                EndValue = new Vector2(0, -10),
                StartTime = startTime,
                EndTime = startTime + beatLength / 2,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = beatLength / 2
            });
            icon.Transforms.Add(new TransformScale
            {
                StartValue = new Vector2(1, 0.9f),
                EndValue = Vector2.One,
                StartTime = startTime,
                EndTime = startTime + beatLength / 2,
                Easing = EasingTypes.Out,
                LoopCount = -1,
                LoopDelay = beatLength / 2
            });
            icon.Transforms.Add(new TransformPosition
            {
                StartValue = new Vector2(0, -10),
                EndValue = Vector2.Zero,
                StartTime = startTime + beatLength / 2,
                EndTime = startTime + beatLength,
                Easing = EasingTypes.In,
                LoopCount = -1,
                LoopDelay = beatLength / 2
            });
            icon.Transforms.Add(new TransformScale
            {
                StartValue = Vector2.One,
                EndValue = new Vector2(1, 0.9f),
                StartTime = startTime + beatLength / 2,
                EndTime = startTime + beatLength,
                Easing = EasingTypes.In,
                LoopCount = -1,
                LoopDelay = beatLength / 2
            });
            icon.Transforms.Add(new TransformRotation
            {
                StartValue = jumpSide ? 10 : -10,
                EndValue = jumpSide ? -10 : 10,
                StartTime = startTime + beatLength,
                EndTime = startTime + beatLength * 2,
                Easing = EasingTypes.InOutSine,
                LoopCount = -1,
                LoopDelay = beatLength
            });
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get($@"Menu/menu-{internalName}-click") ?? audio.Sample.Get(internalName.Contains(@"back") ? @"Menu/menuback" : @"Menu/menuhit");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            boxHoverLayer.FadeTo(0.1f, 1000, EasingTypes.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            boxHoverLayer.FadeTo(0, 1000, EasingTypes.OutQuint);
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
            sampleClick.Play();

            clickAction?.Invoke();

            boxHoverLayer.ClearTransforms();
            boxHoverLayer.Alpha = 0.9f;
            boxHoverLayer.FadeOut(800, EasingTypes.OutExpo);
        }

        public override bool HandleInput => state != ButtonState.Exploded && box.Scale.X >= 0.8f;

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
                        FadeIn(expand_duration / 6f);
                        break;
                    case ButtonState.Exploded:
                        const int explode_duration = 200;
                        box.ScaleTo(new Vector2(2, 1), explode_duration, EasingTypes.OutExpo);
                        FadeOut(explode_duration / 4f * 3);
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
