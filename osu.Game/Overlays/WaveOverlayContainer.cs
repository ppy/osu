// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Overlays
{
    public abstract class WaveOverlayContainer : OverlayContainer
    {
        private const float container_wait = 200;
        private const float waves_container_duration = 400;
        private const float content_duration = 700;
        private const float content_transition_wait = 100;

        internal const float CONTENT_EXIT_DURATION = 600;

        private const float waves_container_position = -150;

        private Wave firstWave, secondWave, thirdWave, fourthWave;

        private Container<Wave> wavesContainer;

        private readonly Container contentContainer;
        protected override Container<Drawable> Content => contentContainer;

        public Color4 FirstWaveColour
        {
            get
            {
                return firstWave.Colour;
            }
            set
            {
                if (firstWave.Colour == value) return;
                firstWave.Colour = value;
            }
        }

        public Color4 SecondWaveColour
        {
            get
            {
                return secondWave.Colour;
            }
            set
            {
                if (secondWave.Colour == value) return;
                secondWave.Colour = value;
            }
        }

        public Color4 ThirdWaveColour
        {
            get
            {
                return thirdWave.Colour;
            }
            set
            {
                if (thirdWave.Colour == value) return;
                thirdWave.Colour = value;
            }
        }

        public Color4 FourthWaveColour
        {
            get
            {
                return fourthWave.Colour;
            }
            set
            {
                if (fourthWave.Colour == value) return;
                fourthWave.Colour = value;
            }
        }

        protected override void PopIn()
        {
            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Visible;

            DelayReset();
            Delay(container_wait);
            Schedule(() =>
            {
                if (State == Visibility.Visible)
                {
                    //wavesContainer.MoveToY(waves_container_position, waves_container_duration, EasingTypes.None);
                    contentContainer.FadeIn(content_duration, EasingTypes.OutQuint);
                    contentContainer.MoveToY(0, content_duration, EasingTypes.OutQuint);

                    Delay(content_transition_wait);
                    Schedule(() => { if (State == Visibility.Visible) TransitionIn(); });
                }
            });
        }

        protected abstract void TransitionIn();

        protected override void PopOut()
        {
            contentContainer.FadeOut(CONTENT_EXIT_DURATION, EasingTypes.InSine);
            contentContainer.MoveToY(DrawHeight, CONTENT_EXIT_DURATION, EasingTypes.InSine);
            TransitionOut();

            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Hidden;

            DelayReset();
            Delay(container_wait);
            //wavesContainer.MoveToY(0, waves_container_duration);
        }

        protected abstract void TransitionOut();

        public WaveOverlayContainer()
        {
            Masking = true;

            const int wave_count = 4;

            const float total_duration = 800;
            const float duration_change = 0;

            float appearDuration = total_duration - wave_count * duration_change;
            float disappearDuration = total_duration;

            AddInternal(wavesContainer = new Container<Wave>
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Children = new[]
                {
                    firstWave = new Wave
                    {
                        Rotation = 13,
                        FinalPosition = -830,
                        TransitionDurationAppear = (appearDuration += duration_change),
                        TransitionDurationDisappear = (disappearDuration -= duration_change),
                    },
                    secondWave = new Wave
                    {
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Rotation = -7,
                        FinalPosition = -460,
                        TransitionDurationAppear = (appearDuration += duration_change),
                        TransitionDurationDisappear = (disappearDuration -= duration_change),
                    },
                    thirdWave = new Wave
                    {
                        Rotation = 4,
                        FinalPosition = -290,
                        TransitionDurationAppear = (appearDuration += duration_change),
                        TransitionDurationDisappear = (disappearDuration -= duration_change),
                    },
                    fourthWave = new Wave
                    {
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Rotation = -2,
                        FinalPosition = -120,
                        TransitionDurationAppear = (appearDuration += duration_change),
                        TransitionDurationDisappear = (disappearDuration -= duration_change),
                    },
                },
            });

            AddInternal(contentContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(50),
                    },
                },
            });
        }

        class Wave : Container, IStateful<Visibility>
        {
            public float FinalPosition;
            public float TransitionDurationAppear;
            public float TransitionDurationDisappear;

            public Wave()
            {
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(1.5f);
                Masking = true;
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(50),
                    Radius = 20f,
                };

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            private Visibility state;
            public Visibility State
            {
                get { return state; }
                set
                {
                    if (value == state) return;
                    state = value;

                    switch (value)
                    {
                        case Visibility.Hidden:
                            MoveToY(DrawHeight / Height, TransitionDurationDisappear, EasingTypes.OutSine);
                            break;
                        case Visibility.Visible:
                            MoveToY(FinalPosition, TransitionDurationAppear, EasingTypes.Out);
                            break;
                    }
                }
            }
        }
    }
}
