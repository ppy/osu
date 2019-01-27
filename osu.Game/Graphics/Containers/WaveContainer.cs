// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Graphics.Containers
{
    public class WaveContainer : VisibilityContainer
    {
        public const float APPEAR_DURATION = 800;
        public const float DISAPPEAR_DURATION = 500;

        private const Easing easing_show = Easing.OutSine;
        private const Easing easing_hide = Easing.InSine;

        private readonly Wave firstWave;
        private readonly Wave secondWave;
        private readonly Wave thirdWave;
        private readonly Wave fourthWave;

        private readonly Container<Wave> wavesContainer;
        private readonly Container contentContainer;

        protected override Container<Drawable> Content => contentContainer;

        public Color4 FirstWaveColour
        {
            get => firstWave.Colour;
            set => firstWave.Colour = value;
        }

        public Color4 SecondWaveColour
        {
            get => secondWave.Colour;
            set => secondWave.Colour = value;
        }

        public Color4 ThirdWaveColour
        {
            get => thirdWave.Colour;
            set => thirdWave.Colour = value;
        }

        public Color4 FourthWaveColour
        {
            get => fourthWave.Colour;
            set => fourthWave.Colour = value;
        }

        public WaveContainer()
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
            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Visible;

            this.FadeIn(100, Easing.OutQuint);
            contentContainer.MoveToY(0, APPEAR_DURATION, Easing.OutQuint);

            this.FadeIn(100, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(DISAPPEAR_DURATION, Easing.InQuint);
            contentContainer.MoveToY(DrawHeight * 2f, DISAPPEAR_DURATION, Easing.In);

            foreach (var w in wavesContainer.Children)
                w.State = Visibility.Hidden;

            this.FadeOut(DISAPPEAR_DURATION, Easing.InQuint);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is done as an optimization, such that invisible parts of the waves
            // are masked away, and thus do not consume fill rate.
            wavesContainer.Height = Math.Max(0, DrawHeight - (contentContainer.DrawHeight - contentContainer.Y));
        }

        private class Wave : VisibilityContainer
        {
            public float FinalPosition;

            protected override bool StartHidden => true;

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

                Child = new Box { RelativeSizeAxes = Axes.Both };
            }

            protected override void Update()
            {
                base.Update();

                // We can not use RelativeSizeAxes for Height, because the height
                // of our parent diminishes as the content moves up.
                Height = Parent.Parent.DrawSize.Y * 1.5f;
            }

            protected override void PopIn() => this.MoveToY(FinalPosition, APPEAR_DURATION, easing_show);
            protected override void PopOut() => this.MoveToY(Parent.Parent.DrawSize.Y, DISAPPEAR_DURATION, easing_hide);
        }
    }
}
