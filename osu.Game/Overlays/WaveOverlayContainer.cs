// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private const float first_wave_position = -130;
        private const float second_wave_position = 0;
        private const float third_wave_position = 70;
        private const float fourth_wave_position = 100;
        private const float waves_container_position = -150;
        private float[] wave_positions
        {
            get
            {
                return new float[] { first_wave_position, second_wave_position, third_wave_position, fourth_wave_position };
            }
        }

        private const float first_wave_duration = 600;
        private const float second_wave_duration = 700;
        private const float third_wave_duration = 800;
        private const float fourth_wave_duration = 900;
        private const float container_wait = 200;
        private const float waves_container_duration = 400;
        private const float content_duration = 700;
        private const float content_transition_wait = 100;
        internal const float content_exit_duration = 600;
        private float [] wave_durations
        {
            get
            {
                return new float[] { first_wave_duration, second_wave_duration, third_wave_duration, fourth_wave_duration };
            }
        }

        private const float first_wave_rotation = 13;
        private const float second_wave_rotation = -7;
        private const float third_wave_rotation = 4;
        private const float fourth_wave_rotation = -2;

        private Container firstWave, secondWave, thirdWave, fourthWave, wavesContainer;
        private readonly Container[] waves;

        private readonly Container contentContainer;
        protected override Container<Drawable> Content => contentContainer;

        private EdgeEffect waveShadow = new EdgeEffect
        {
            Type = EdgeEffectType.Shadow,
            Colour = Color4.Black.Opacity(50),
            Radius = 20f,
        };

        private Color4 firstWaveColour;
        public Color4 FirstWaveColour
        {
            get
            {
                return firstWaveColour;
            }
            set
            {
                if (firstWaveColour == value) return;
                firstWaveColour = value;
                firstWave.Colour = value;
            }
        }

        private Color4 secondWaveColour;
        public Color4 SecondWaveColour
        {
            get
            {
                return secondWaveColour;
            }
            set
            {
                if (secondWaveColour == value) return;
                secondWaveColour = value;
                secondWave.Colour = value;
            }
        }

        private Color4 thirdWaveColour;
        public Color4 ThirdWaveColour
        {
            get
            {
                return thirdWaveColour;
            }
            set
            {
                if (thirdWaveColour == value) return;
                thirdWaveColour = value;
                thirdWave.Colour = value;
            }
        }

        private Color4 fourthWaveColour;
        public Color4 FourthWaveColour
        {
            get
            {
                return fourthWaveColour;
            }
            set
            {
                if (fourthWaveColour == value) return;
                fourthWaveColour = value;
                fourthWave.Colour = value;
            }
        }

        // TODO: Remove when framework updated
        public override bool HandleInput => State == Visibility.Visible;

        protected override void PopIn()
        {
            base.Show();

            for (int i = 0; i < waves.Length; i++)
            {
                waves[i].MoveToY(wave_positions[i], wave_durations[i], EasingTypes.OutQuint);
            }

            DelayReset();
            Delay(container_wait);
            Schedule(() =>
            {
                if (State == Visibility.Visible)
                {
                    wavesContainer.MoveToY(waves_container_position, waves_container_duration, EasingTypes.None);
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
            base.Hide();

            contentContainer.FadeOut(content_exit_duration, EasingTypes.InSine);
            contentContainer.MoveToY(DrawHeight, content_exit_duration, EasingTypes.InSine);
            TransitionOut();

            for (int i = 0; i < waves.Length; i++)
            {
                waves[i].MoveToY(DrawHeight, second_wave_duration, EasingTypes.InSine);
            }

            DelayReset();
            Delay(container_wait);
            Schedule(() =>
            {
                if (State == Visibility.Hidden)
                {
                    wavesContainer.MoveToY(0, waves_container_duration, EasingTypes.None);
                }
            });
        }

        protected abstract void TransitionOut();

        public WaveOverlayContainer()
        {
            Masking = true;

            AddInternal(wavesContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Children = new Drawable[]
                {
                    firstWave = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Size = new Vector2(1.5f),
                        Rotation = first_wave_rotation,
                        Colour = FirstWaveColour,
                        Masking = true,
                        EdgeEffect = waveShadow,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                    secondWave = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Size = new Vector2(1.5f),
                        Rotation = second_wave_rotation,
                        Colour = SecondWaveColour,
                        Masking = true,
                        EdgeEffect = waveShadow,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                    thirdWave = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Size = new Vector2(1.5f),
                        Rotation = third_wave_rotation,
                        Colour = ThirdWaveColour,
                        Masking = true,
                        EdgeEffect = waveShadow,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
                    },
                    fourthWave = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Size = new Vector2(1.5f),
                        Rotation = fourth_wave_rotation,
                        Colour = FourthWaveColour,
                        Masking = true,
                        EdgeEffect = waveShadow,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        },
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

            waves = new Container[] { firstWave, secondWave, thirdWave, fourthWave };
        }
    }
}
