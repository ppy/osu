// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
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

        protected override bool StartHidden => true;

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
                RelativePositionAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            });
        }

        protected override void PopIn()
        {
            foreach (var w in wavesContainer.Children)
                w.Show();

            contentContainer.MoveToY(0, APPEAR_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            foreach (var w in wavesContainer.Children)
                w.Hide();

            contentContainer.MoveToY(2, DISAPPEAR_DURATION, Easing.In);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is done as an optimization, such that invisible parts of the waves
            // are masked away, and thus do not consume fill rate.
            // todo: revert https://github.com/ppy/osu/commit/aff9e3617da0c8fe252169fae287e39b44575b5e after FTB is fixed on iOS.
            wavesContainer.Height = Math.Max(0, DrawHeight - (contentContainer.DrawHeight - contentContainer.Y * DrawHeight));
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

            protected override void PopIn() => Schedule(() => this.MoveToY(FinalPosition, APPEAR_DURATION, easing_show));

            protected override void PopOut()
            {
                double duration = IsLoaded ? DISAPPEAR_DURATION : 0;

                // scheduling is required as parent may not be present at the time this is called.
                Schedule(() => this.MoveToY(Parent.Parent.DrawSize.Y, duration, easing_hide));
            }
        }
    }
}
