﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Overlays
{
    public abstract class WaveOverlayContainer : FocusedOverlayContainer
    {
        protected const float APPEAR_DURATION = 800;
        protected const float DISAPPEAR_DURATION = 500;

        private const EasingTypes easing_show = EasingTypes.OutSine;
        private const EasingTypes easing_hide = EasingTypes.InSine;

        private readonly Wave firstWave;
        private readonly Wave secondWave;
        private readonly Wave thirdWave;
        private readonly Wave fourthWave;

        private readonly Container<Wave> wavesContainer;

        private readonly Container contentContainer;

        protected override Container<Drawable> Content => contentContainer;

        protected Color4 FirstWaveColour
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

        protected Color4 SecondWaveColour
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

        protected Color4 ThirdWaveColour
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

        protected Color4 FourthWaveColour
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

        protected WaveOverlayContainer()
        {
            Masking = true;

            AddInternal(wavesContainer = new Container<Wave>
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Masking = true,
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
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Visible;

            FadeIn(100, EasingTypes.OutQuint);
            contentContainer.MoveToY(0, APPEAR_DURATION, EasingTypes.OutQuint);

            FadeIn(100, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            FadeOut(DISAPPEAR_DURATION, EasingTypes.InQuint);
            contentContainer.MoveToY(DrawHeight * 2f, DISAPPEAR_DURATION, EasingTypes.In);

            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Hidden;

            FadeOut(DISAPPEAR_DURATION, EasingTypes.InQuint);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is done as an optimization, such that invisible parts of the waves
            // are masked away, and thus do not consume fill rate.
            wavesContainer.Height = Math.Max(0, DrawHeight - (contentContainer.DrawHeight - contentContainer.Y));
        }

        private class Wave : Container, IStateful<Visibility>
        {
            public float FinalPosition;

            public Wave()
            {
                RelativeSizeAxes = Axes.X;
                Width = 1.5f;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
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

            protected override void Update()
            {
                base.Update();

                // We can not use RelativeSizeAxes for Height, because the height
                // of our parent diminishes as the content moves up.
                Height = Parent.Parent.DrawSize.Y * 1.5f;
            }

            private Visibility state;
            public Visibility State
            {
                get { return state; }
                set
                {
                    state = value;

                    switch (value)
                    {
                        case Visibility.Hidden:
                            MoveToY(Parent.Parent.DrawSize.Y, DISAPPEAR_DURATION, easing_hide);
                            break;
                        case Visibility.Visible:
                            MoveToY(FinalPosition, APPEAR_DURATION, easing_show);
                            break;
                    }
                }
            }
        }
    }
}
