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
        protected const float CONTENT_DURATION = 800;

        private const EasingTypes easing_show = EasingTypes.OutSine;
        private const EasingTypes easing_hide = EasingTypes.InSine;

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

        public WaveOverlayContainer()
        {
            Masking = true;

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
                        FinalPosition = -930,
                    },
                    secondWave = new Wave
                    {
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Rotation = -7,
                        FinalPosition = -560,
                    },
                    thirdWave = new Wave
                    {
                        Rotation = 4,
                        FinalPosition = -390,
                    },
                    fourthWave = new Wave
                    {
                        Origin = Anchor.TopRight,
                        Anchor = Anchor.TopRight,
                        Rotation = -2,
                        FinalPosition = -220,
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

        protected override void PopIn()
        {
            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Visible;

            contentContainer.FadeIn(CONTENT_DURATION, EasingTypes.OutQuint);
            contentContainer.MoveToY(0, CONTENT_DURATION, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            contentContainer.FadeOut(CONTENT_DURATION, easing_hide);
            contentContainer.MoveToY(DrawHeight * 2f, CONTENT_DURATION, easing_hide);

            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Hidden;
        }

        class Wave : Container, IStateful<Visibility>
        {
            public float FinalPosition;

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
                            MoveToY(DrawHeight / Height, CONTENT_DURATION, easing_hide);
                            break;
                        case Visibility.Visible:
                            MoveToY(FinalPosition, CONTENT_DURATION, easing_show);
                            break;
                    }
                }
            }
        }
    }
}
